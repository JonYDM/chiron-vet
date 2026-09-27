using Chiron.Application.Seguridad;

namespace Chiron.Infrastructure.Seguridad;

/// <summary>
/// Implementación de IHasheadorContrasena usando BCrypt.
/// BCrypt genera un salt aleatorio por contraseña y es resistente a fuerza bruta
/// (factor de trabajo ajustable). Estándar recomendado para almacenar contraseñas.
/// </summary>
public sealed class HasheadorBCrypt : IHasheadorContrasena
{
    public string Hashear(string contrasena) => BCrypt.Net.BCrypt.HashPassword(contrasena);

    public bool Verificar(string contrasena, string hash)
    {
        // BCrypt.Verify lanza si el hash tiene formato inválido; lo tratamos como no válido.
        try
        {
            return BCrypt.Net.BCrypt.Verify(contrasena, hash);
        }
        catch
        {
            return false;
        }
    }
}
