using Chiron.Domain.Common;

namespace Chiron.Application.Seguridad;

/// <summary>Validación del formato del PIN, reutilizable por los casos de uso.</summary>
public static class ValidadorPin
{
    /// <summary>Longitud requerida del PIN.</summary>
    public const int Longitud = 6;

    /// <summary>Verifica que el PIN tenga exactamente 6 dígitos numéricos.</summary>
    public static Result<bool> Validar(string? pin)
    {
        if (string.IsNullOrWhiteSpace(pin))
            return Result<bool>.Falla("El PIN es obligatorio.");
        if (pin.Length != Longitud)
            return Result<bool>.Falla($"El PIN debe tener {Longitud} dígitos.");

        foreach (char c in pin)
            if (!char.IsDigit(c))
                return Result<bool>.Falla("El PIN debe contener solo dígitos.");

        return Result<bool>.Exito(true);
    }
}
