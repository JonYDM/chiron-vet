namespace Chiron.Domain.Common;

/// <summary>
/// Representa el resultado de una operación que puede fallar por reglas de negocio.
/// Evita usar excepciones para control de flujo (más eficiente y explícito):
/// las excepciones se reservan para errores realmente excepcionales, no para
/// validaciones esperadas como "el nombre está vacío".
/// </summary>
/// <typeparam name="T">Tipo del valor producido cuando la operación es exitosa.</typeparam>
public readonly struct Result<T>
{
    /// <summary>Indica si la operación fue exitosa.</summary>
    public bool EsExito { get; }

    /// <summary>Valor resultante (solo válido cuando EsExito es true).</summary>
    public T? Valor { get; }

    /// <summary>Mensaje de error (solo válido cuando EsExito es false).</summary>
    public string? Error { get; }

    private Result(bool esExito, T? valor, string? error)
    {
        EsExito = esExito;
        Valor = valor;
        Error = error;
    }

    /// <summary>Crea un resultado exitoso con el valor indicado.</summary>
    public static Result<T> Exito(T valor) => new(true, valor, null);

    /// <summary>Crea un resultado fallido con el mensaje de error indicado.</summary>
    public static Result<T> Falla(string error) => new(false, default, error);
}
