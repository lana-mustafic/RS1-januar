using Market.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace Market.Infrastructure.Database.Configurations.Sales;

public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder
            .ToTable("Oders");

        builder
            .Property(x => x.ReferenceNumber)
            .HasMaxLength(OrderEntity.Constraints.ReferenceMaxLength);

        builder
          .HasOne(x => x.MarketUser)
          .WithMany() // ako nemamo navigaciju, onda stavimo samo WithMany()
          .HasForeignKey(x => x.MarketUserId)
          .OnDelete(DeleteBehavior.Restrict);// Restrict — do not allow deleting a MarketUser if it has Order

    }
}
