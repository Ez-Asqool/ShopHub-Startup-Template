using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using myshop.Domain.Entities;

namespace myshop.Infrastructure.Data.Configurations
{
    public class OrderHeaderConfiguration : IEntityTypeConfiguration<OrderHeader>
    {
        public void Configure(EntityTypeBuilder<OrderHeader> builder)
        {
            builder.ToTable("OrderHeaders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.ApplicationUserId)
                .IsRequired();

            builder.Property(o => o.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.OrderStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(o => o.PaymentStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(o => o.RecipientName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(o => o.Address)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(o => o.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.PhoneNumber)
                .HasMaxLength(20);

            builder.ConfigureBaseEntity();
        }
    }
}
