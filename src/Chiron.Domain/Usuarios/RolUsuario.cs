namespace Chiron.Domain.Usuarios;

/// <summary>
/// Rol de un usuario dentro de una veterinaria. Define qué puede hacer en el sistema.
/// Los permisos concretos se aplicarán en la capa de aplicación/API (Épica 8).
/// </summary>
public enum RolUsuario
{
    /// <summary>Dueño/gerente: acceso total, incluidos reportes de dinero.</summary>
    Administrador = 1,

    /// <summary>Atiende pacientes: ve y edita expedientes clínicos.</summary>
    Veterinario = 2,

    /// <summary>Recepción: agenda citas, registra clientes y mascotas.</summary>
    Recepcionista = 3
}
