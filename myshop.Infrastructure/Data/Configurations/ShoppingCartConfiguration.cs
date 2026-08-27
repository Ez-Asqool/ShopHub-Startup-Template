using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using myshop.Domain.Entities;

namespace myshop.Infrastructure.Data.Configurations
{
    public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
    {
        public void Configure(EntityTypeBuilder<ShoppingCart> builder)
        {
            builder.ToTable("ShoppingCarts");

            builder.HasKey(sc => sc.Id);

            builder.Property(sc => sc.Count)
                .IsRequired();

            builder.Property(sc => sc.ApplicationUserId)
                .IsRequired();

            builder.HasOne(sc => sc.Product)
                .WithMany()
                .HasForeignKey(sc => sc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ConfigureBaseEntity();
        }
    }
}
