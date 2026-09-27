using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Chiron.Application.Seguridad;
using Microsoft.IdentityModel.Tokens;

namespace Chiron.Infrastructure.Seguridad;

/// <summary>
/// Implementación de IGeneradorToken que emite JWT firmados (HMAC-SHA256).
/// El token incluye claims con el Id, veterinaria (tenant), correo y rol del usuario.
/// </summary>
public sealed class GeneradorTokenJwt : IGeneradorToken
{
    private readonly JwtOpciones _opciones;

    public GeneradorTokenJwt(JwtOpciones opciones) => _opciones = opciones;

    public (string token, DateTime expiraEn) Generar(DatosToken datos)
    {
        DateTime expiraEn = DateTime.UtcNow.AddMinutes(_opciones.MinutosValidez);

        // Claims: información que viaja dentro del token (el backend la lee para autorizar).
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, datos.UsuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, datos.Correo),
            new Claim("veterinariaId", datos.VeterinariaId.ToString()),
            new Claim(ClaimTypes.Role, datos.Rol.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Clave));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Emisor,
            audience: _opciones.Audiencia,
            claims: claims,
            expires: expiraEn,
            signingCredentials: credenciales);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expiraEn);
    }
}
