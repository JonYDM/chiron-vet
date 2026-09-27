using Chiron.Domain.Clientes;
using Chiron.Domain.Mascotas;

namespace Chiron.Application.Clientes;

/// <summary>
/// Datos de entrada para el registro rápido de un cliente junto con su primera mascota.
/// Representa el escenario típico de recepción: llega un cliente nuevo con su mascota.
/// </summary>
/// <param name="VeterinariaId">Veterinaria (tenant) donde se registra.</param>
/// <param name="NombreCliente">Nombre del dueño.</param>
/// <param name="TelefonoCliente">Teléfono del dueño.</param>
/// <param name="OrigenCliente">Cómo conoció la veterinaria (opcional).</param>
/// <param name="NombreMascota">Nombre de la mascota.</param>
/// <param name="Especie">Especie de la mascota.</param>
/// <param name="Sexo">Sexo de la mascota (opcional).</param>
/// <param name="Raza">Raza de la mascota (opcional).</param>
/// <param name="FechaNacimiento">Fecha de nacimiento de la mascota (opcional).</param>
public sealed record RegistrarClienteConMascotaComando(
    Guid VeterinariaId,
    string NombreCliente,
    string TelefonoCliente,
    OrigenCliente OrigenCliente,
    string NombreMascota,
    EspecieMascota Especie,
    SexoMascota Sexo = SexoMascota.NoEspecificado,
    string? Raza = null,
    DateOnly? FechaNacimiento = null);
