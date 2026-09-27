using Chiron.Domain.Common;

namespace Chiron.Domain.Common;

/// <summary>
/// Entidad de prueba temporal usada únicamente para validar la fundación técnica
/// (repositorio genérico + DI) en la Épica 1.
/// Será eliminada cuando lleguen las entidades reales del negocio (Épica 2).
/// </summary>
public sealed class EntidadPrueba : EntidadBase
{
    public string Nombre { get; }

    public EntidadPrueba(string nombre)
    {
        Nombre = nombre;
    }
}
