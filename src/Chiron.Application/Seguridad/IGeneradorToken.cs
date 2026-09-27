using Chiron.Domain.Usuarios;

namespace Chiron.Application.Seguridad;

/// <summary>
/// Datos que se incluirán en el token para identificar al usuario y autorizar.
/// </summary>
/// <param name="UsuarioId">Id del usuario.</param>
/// <param name="VeterinariaId">Tenant al que pertenece (aislamiento multi-tenant).</param>
/// <param name="Correo">Correo del usuario.</param>
/// <param name="Rol">Rol para autorización.</param>
public sealed record DatosToken(Guid UsuarioId, Guid VeterinariaId, string Correo, RolUsuario Rol);

/// <summary>
/// Abstracción para generar tokens JWT firmados. La implementación (con la clave secreta
/// y la librería de JWT) vive en Infrastructure.
/// </summary>
public interface IGeneradorToken
{
    /// <summary>Genera un JWT firmado con los datos del usuario. Devuelve el token y su expiración (UTC).</summary>
    (string token, DateTime expiraEn) Generar(DatosToken datos);
}
