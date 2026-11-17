using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TaskManager.Infrastructure.Persistence.Converters;

/// <summary>Marca como UTC las fechas leídas de la base de datos.</summary>
internal class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value,
    value => AsUtc(value))
{
    public static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
