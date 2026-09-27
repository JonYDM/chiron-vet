namespace Chiron.Application.Seguridad;

/// <summary>
/// Abstracción para hashear y verificar contraseñas. La implementación concreta
/// (BCrypt, PBKDF2, etc.) vive en Infrastructure. El dominio/aplicación no dependen
/// del algoritmo específico (inversión de dependencias).
/// </summary>
public interface IHasheadorContrasena
{
    /// <summary>Genera el hash de una contraseña en texto plano.</summary>
    string Hashear(string contrasena);

    /// <summary>Verifica que una contraseña en texto plano corresponda al hash dado.</summary>
    bool Verificar(string contrasena, string hash);
}
