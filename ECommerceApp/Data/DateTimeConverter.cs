using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ECommerceApp.Data
{
    public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter() : base(
                v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            )
            { 
            }
    }
    public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public NullableUtcDateTimeConverter() : base(
                v => v == null ? 
                            null : (v.Value.Kind == DateTimeKind.Unspecified ? 
                                DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()),
                v => v == null ? null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
            )
            { 
            }
    }
}
