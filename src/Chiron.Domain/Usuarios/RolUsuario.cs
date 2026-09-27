namespace Chiron.Domain.Usuarios;

/// <summary>
/// Rol de un usuario. Define qué puede hacer en el sistema. Los permisos concretos
/// se aplican en la capa de API mediante autorización por rol.
/// </summary>
public enum RolUsuario
{
    /// <summary>Dueño/gerente de la veterinaria: acceso total dentro de su tenant.</summary>
    Administrador = 1,

    /// <summary>Atiende pacientes: ve y edita expedientes clínicos.</summary>
    Veterinario = 2,

    /// <summary>Recepción: agenda citas, registra clientes y mascotas.</summary>
    Recepcionista = 3,

    /// <summary>Cliente final (dueño de mascota): acceso limitado a SUS mascotas e historial.</summary>
    DuenoMascota = 4,

    /// <summary>SuperAdmin (dueño de Chiron): gestiona veterinarias y suscripciones. Por encima de los tenants.</summary>
    SuperAdmin = 99
}
