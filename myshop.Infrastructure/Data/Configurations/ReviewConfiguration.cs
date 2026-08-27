using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using myshop.Domain.Entities;

namespace myshop.Infrastructure.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.ApplicationUserId)
                .IsRequired();

            builder.Property(r => r.ReviewerName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Comment)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ConfigureBaseEntity();
        }
    }
}
