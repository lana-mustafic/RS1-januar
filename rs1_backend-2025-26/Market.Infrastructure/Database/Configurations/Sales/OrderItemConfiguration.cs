using Market.Domain.Entities.Sales;

namespace Market.Infrastructure.Database.Configurations.Sales;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemEntity>
{
    public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
    {
        builder
            .ToTable("OrderItems");

        builder
          .HasOne(x => x.Product)
          .WithMany() // ako nemamo navigaciju, onda stavimo samo WithMany()
          .HasForeignKey(x => x.ProductId)
          .OnDelete(DeleteBehavior.Restrict);// Restrict — do not allow deleting a Product if it has OrderItems


        builder
            .HasOne(x => x.Order)
            .WithMany(x=>x.Items) // ako nemamo navigaciju, onda stavimo samo WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);// Cascade — deleting a Order will delete OrderItems
    }
}