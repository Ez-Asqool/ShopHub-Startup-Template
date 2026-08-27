using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using myshop.Domain.Common;

namespace myshop.Infrastructure.Data.Configurations
{
    public static class EntityTypeBuilderExtensions
    {
        public static void ConfigureBaseEntity<T>(this EntityTypeBuilder<T> builder) where T : BaseEntity
        {
            builder.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.UpdatedAt);
        }
    }
}
