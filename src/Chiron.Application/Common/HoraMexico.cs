namespace Chiron.Application.Common;

/// <summary>
/// "Hoy" y el inicio del día en hora de México (las clínicas operan en esa zona).
/// Las fechas se guardan en UTC; usar <c>DateTime.UtcNow</c> para decidir "hoy" hace que
/// después de las 6 pm (UTC-6) el día ya cuente como el siguiente y desaparezcan
/// los recordatorios de hoy.
/// </summary>
public static class HoraMexico
{
    /// <summary>Zona IANA (Linux/Railway) con respaldo al id de Windows.</summary>
    public static readonly TimeZoneInfo Zona = ObtenerZona();

    private static TimeZoneInfo ObtenerZona()
    {
        foreach (string id in new[] { "America/Mexico_City", "Central Standard Time (Mexico)" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        // Último recurso: México no tiene horario de verano desde 2022 (UTC-6 fijo).
        return TimeZoneInfo.CreateCustomTimeZone("Mexico-6", TimeSpan.FromHours(-6), "México", "México");
    }

    /// <summary>Convierte un instante UTC a hora local de México.</summary>
    public static DateTime ALocal(DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zona);

    /// <summary>Fecha de hoy en México.</summary>
    public static DateOnly Hoy() => DateOnly.FromDateTime(ALocal(DateTime.UtcNow));

    /// <summary>Instante UTC en que empezó el día de hoy en México (00:00 local).</summary>
    public static DateTime InicioDeHoyUtc() => AUtc(Hoy());

    /// <summary>Instante UTC en que empezó el mes en curso en México (día 1, 00:00 local).</summary>
    public static DateTime InicioDeMesUtc()
    {
        DateOnly hoy = Hoy();
        return AUtc(new DateOnly(hoy.Year, hoy.Month, 1));
    }

    /// <summary>¿El instante UTC cae en el mes en curso de México?</summary>
    public static bool EsDelMesActual(DateTime utc)
    {
        DateTime local = ALocal(utc);
        DateOnly hoy = Hoy();
        return local.Year == hoy.Year && local.Month == hoy.Month;
    }

    /// <summary>Medianoche local de una fecha, expresada en UTC.</summary>
    private static DateTime AUtc(DateOnly fechaLocal)
        => TimeZoneInfo.ConvertTimeToUtc(fechaLocal.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), Zona);
}
