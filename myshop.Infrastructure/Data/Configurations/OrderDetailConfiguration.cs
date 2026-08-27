using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using myshop.Domain.Entities;

namespace myshop.Infrastructure.Data.Configurations
{
    public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.ToTable("OrderDetails");

            builder.HasKey(od => od.Id);

            builder.Property(od => od.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(od => od.Quantity)
                .IsRequired();

            builder.HasOne(od => od.OrderHeader)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ConfigureBaseEntity();
        }
    }
}
