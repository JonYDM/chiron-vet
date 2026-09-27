using Chiron.Domain.Common;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Mascotas;

/// <summary>Datos para subir una foto de mascota.</summary>
public sealed record SubirFotoComando(
    Guid VeterinariaId,
    Guid MascotaId,
    byte[] Contenido,
    Guid SubidaPorUsuarioId,
    Guid? RegistroMedicoId = null);

/// <summary>Foto de mascota en formato de salida (para la galería).</summary>
public sealed record FotoMascotaDto(
    Guid Id,
    Guid MascotaId,
    Guid? RegistroMedicoId,
    string Url,
    DateTime FechaSubida);

/// <summary>
/// Casos de uso de la galería de fotos de una mascota: subir, listar y eliminar.
/// La subida delega el almacenamiento del binario en IAlmacenamientoArchivos
/// (que comprime y sube a R2) y persiste solo la referencia en la base de datos.
/// </summary>
public sealed class GestionFotoMascota
{
    // Límite defensivo del archivo de entrada (5 MB). La compresión posterior
    // deja la imagen final en decenas de KB, pero validamos el tamaño de entrada
    // para no aceptar cargas abusivas.
    private const int MaxBytesEntrada = 5 * 1024 * 1024;

    private readonly IFotoMascotaRepository _fotos;
    private readonly IMascotaRepository _mascotas;
    private readonly IAlmacenamientoArchivos _almacenamiento;

    public GestionFotoMascota(
        IFotoMascotaRepository fotos,
        IMascotaRepository mascotas,
        IAlmacenamientoArchivos almacenamiento)
    {
        _fotos = fotos;
        _mascotas = mascotas;
        _almacenamiento = almacenamiento;
    }

    /// <summary>Sube una foto a la galería de la mascota.</summary>
    public async Task<Result<FotoMascotaDto>> SubirAsync(
        SubirFotoComando cmd, CancellationToken ct = default)
    {
        if (cmd.Contenido is null || cmd.Contenido.Length == 0)
            return Result<FotoMascotaDto>.Falla("La imagen está vacía.");
        if (cmd.Contenido.Length > MaxBytesEntrada)
            return Result<FotoMascotaDto>.Falla("La imagen supera el tamaño máximo permitido (5 MB).");

        // La mascota debe existir y pertenecer a la veterinaria del solicitante (aislamiento tenant).
        Mascota? mascota = await _mascotas.ObtenerPorIdAsync(cmd.MascotaId, ct);
        if (mascota is null || mascota.VeterinariaId != cmd.VeterinariaId)
            return Result<FotoMascotaDto>.Falla("La mascota no existe en tu veterinaria.");

        // Sube el binario al almacenamiento (comprime + guarda en R2).
        ArchivoSubido subido = await _almacenamiento.SubirImagenAsync(
            cmd.Contenido, prefijo: $"mascotas/{cmd.MascotaId}", ct);

        Result<FotoMascota> creada = FotoMascota.Crear(
            cmd.VeterinariaId, cmd.MascotaId, subido.Clave, subido.Url,
            cmd.SubidaPorUsuarioId, cmd.RegistroMedicoId);
        if (!creada.EsExito)
        {
            // Si por alguna razón no se puede persistir, limpiamos el objeto huérfano.
            await _almacenamiento.EliminarAsync(subido.Clave, ct);
            return Result<FotoMascotaDto>.Falla(creada.Error!);
        }

        await _fotos.AgregarAsync(creada.Valor!, ct);
        return Result<FotoMascotaDto>.Exito(ToDto(creada.Valor!));
    }

    /// <summary>Lista la galería de una mascota (más recientes primero).</summary>
    public async Task<Result<IReadOnlyList<FotoMascotaDto>>> ListarAsync(
        Guid veterinariaId, Guid mascotaId, CancellationToken ct = default)
    {
        Mascota? mascota = await _mascotas.ObtenerPorIdAsync(mascotaId, ct);
        if (mascota is null || mascota.VeterinariaId != veterinariaId)
            return Result<IReadOnlyList<FotoMascotaDto>>.Falla("La mascota no existe en tu veterinaria.");

        IReadOnlyList<FotoMascota> fotos = await _fotos.ListarPorMascotaAsync(mascotaId, ct);
        IReadOnlyList<FotoMascotaDto> dtos = fotos.Select(ToDto).ToList();
        return Result<IReadOnlyList<FotoMascotaDto>>.Exito(dtos);
    }

    /// <summary>Elimina una foto (del almacenamiento y de la base de datos).</summary>
    public async Task<Result<bool>> EliminarAsync(
        Guid veterinariaId, Guid mascotaId, Guid fotoId, CancellationToken ct = default)
    {
        FotoMascota? foto = await _fotos.ObtenerPorIdAsync(fotoId, ct);
        if (foto is null || foto.MascotaId != mascotaId || foto.VeterinariaId != veterinariaId)
            return Result<bool>.Falla("La foto no existe en tu veterinaria.");

        await _almacenamiento.EliminarAsync(foto.ClaveObjeto, ct);
        await _fotos.EliminarAsync(fotoId, ct);
        return Result<bool>.Exito(true);
    }

    private static FotoMascotaDto ToDto(FotoMascota f) =>
        new(f.Id, f.MascotaId, f.RegistroMedicoId, f.Url, f.FechaSubida);
}
