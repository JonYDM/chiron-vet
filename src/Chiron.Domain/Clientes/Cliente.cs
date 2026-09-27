using Chiron.Domain.Common;

namespace Chiron.Domain.Clientes;

/// <summary>
/// Cliente (dueño de una o más mascotas) de la veterinaria.
///
/// Diseño rico (no anémico): las reglas de negocio viven dentro de la entidad.
/// El constructor es privado; la única forma de crear un Cliente es mediante
/// la fábrica <see cref="Crear"/>, que valida las reglas. Así es imposible
/// tener un Cliente en estado inválido.
/// </summary>
public sealed class Cliente : EntidadBase
{
    /// <summary>Longitud mínima aceptada para el teléfono (formato LATAM, típicamente 10 dígitos).</summary>
    private const int LongitudMinimaTelefono = 10;

    /// <summary>Veterinaria (tenant) a la que pertenece el cliente. Aislamiento multi-tenant.</summary>
    public Guid VeterinariaId { get; private set; }

    /// <summary>Nombre completo del cliente.</summary>
    public string Nombre { get; private set; }

    /// <summary>Teléfono del cliente (solo dígitos). Clave para recordatorios por WhatsApp.</summary>
    public string Telefono { get; private set; }

    /// <summary>Fecha en que el cliente fue registrado (UTC). Base para métricas de crecimiento.</summary>
    public DateTime FechaRegistro { get; private set; }

    /// <summary>Canal por el que el cliente conoció la veterinaria (métricas de marketing).</summary>
    public OrigenCliente Origen { get; private set; }

    /// <summary>
    /// Indica si el cliente dio su consentimiento (opt-in) para recibir mensajes por WhatsApp.
    /// Requisito OBLIGATORIO de Meta: no se puede enviar recordatorios sin este consentimiento.
    /// Por defecto es false hasta que el cliente lo autorice explícitamente.
    /// </summary>
    public bool AceptaWhatsApp { get; private set; }

    /// <summary>Indica si el cliente está activo (baja lógica; preserva su historial).</summary>
    public bool Activo { get; private set; }

    // Constructor privado: fuerza el uso de la fábrica Crear para garantizar validez.
    private Cliente(Guid veterinariaId, string nombre, string telefono, OrigenCliente origen, bool aceptaWhatsApp)
    {
        VeterinariaId = veterinariaId;
        Nombre = nombre;
        Telefono = telefono;
        Origen = origen;
        AceptaWhatsApp = aceptaWhatsApp;
        FechaRegistro = DateTime.UtcNow;
        Activo = true;
    }

    /// <summary>
    /// Crea un Cliente validando las reglas de negocio.
    /// Devuelve un Result: éxito con el Cliente, o falla con el motivo.
    /// </summary>
    /// <param name="veterinariaId">Veterinaria (tenant) dueña del registro (obligatorio).</param>
    /// <param name="nombre">Nombre completo (obligatorio).</param>
    /// <param name="telefono">Teléfono (obligatorio, mínimo 10 dígitos).</param>
    /// <param name="origen">Canal de captación (opcional).</param>
    /// <param name="aceptaWhatsApp">Consentimiento para recibir WhatsApp (opcional, por defecto false).</param>
    public static Result<Cliente> Crear(
        Guid veterinariaId,
        string nombre,
        string telefono,
        OrigenCliente origen = OrigenCliente.NoEspecificado,
        bool aceptaWhatsApp = false)
    {
        if (veterinariaId == Guid.Empty)
            return Result<Cliente>.Falla("El cliente debe pertenecer a una veterinaria válida.");

        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Cliente>.Falla("El nombre del cliente es obligatorio.");

        if (string.IsNullOrWhiteSpace(telefono))
            return Result<Cliente>.Falla("El teléfono del cliente es obligatorio.");

        // Normaliza el teléfono dejando solo dígitos (quita espacios, guiones, paréntesis).
        string telefonoNormalizado = NormalizarTelefono(telefono);

        if (telefonoNormalizado.Length < LongitudMinimaTelefono)
            return Result<Cliente>.Falla(
                $"El teléfono debe tener al menos {LongitudMinimaTelefono} dígitos.");

        // Se recorta el nombre para evitar espacios sobrantes al inicio/fin.
        var cliente = new Cliente(veterinariaId, nombre.Trim(), telefonoNormalizado, origen, aceptaWhatsApp);
        return Result<Cliente>.Exito(cliente);
    }

    /// <summary>Registra el consentimiento del cliente para recibir WhatsApp.</summary>
    public void OtorgarConsentimientoWhatsApp() => AceptaWhatsApp = true;

    /// <summary>Da de baja lógica al cliente (preserva historial de mascotas/ventas).</summary>
    public void Desactivar() => Activo = false;

    /// <summary>Reactiva un cliente dado de baja.</summary>
    public void Activar() => Activo = true;

    /// <summary>
    /// Actualiza los datos editables del cliente (nombre, teléfono, origen), aplicando
    /// las mismas validaciones que la creación.
    /// </summary>
    public Result<bool> ActualizarDatos(string nombre, string telefono, OrigenCliente origen)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<bool>.Falla("El nombre del cliente es obligatorio.");
        if (string.IsNullOrWhiteSpace(telefono))
            return Result<bool>.Falla("El teléfono del cliente es obligatorio.");

        string telefonoNormalizado = NormalizarTelefono(telefono);
        if (telefonoNormalizado.Length < LongitudMinimaTelefono)
            return Result<bool>.Falla(
                $"El teléfono debe tener al menos {LongitudMinimaTelefono} dígitos.");

        Nombre = nombre.Trim();
        Telefono = telefonoNormalizado;
        Origen = origen;
        return Result<bool>.Exito(true);
    }

    /// <summary>Revoca el consentimiento del cliente para recibir WhatsApp.</summary>
    public void RevocarConsentimientoWhatsApp() => AceptaWhatsApp = false;

    /// <summary>
    /// Deja en el teléfono únicamente los dígitos.
    /// Recorre una sola vez la cadena (O(n)) construyendo el resultado.
    /// </summary>
    private static string NormalizarTelefono(string telefono)
    {
        // Capacidad inicial = longitud de entrada para evitar realojos del buffer.
        var soloDigitos = new System.Text.StringBuilder(telefono.Length);
        foreach (char c in telefono)
        {
            if (char.IsDigit(c))
                soloDigitos.Append(c);
        }
        return soloDigitos.ToString();
    }
}
