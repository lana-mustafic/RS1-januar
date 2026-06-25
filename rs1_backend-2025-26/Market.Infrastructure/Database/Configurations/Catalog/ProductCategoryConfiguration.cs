namespace Market.Infrastructure.Database.Configurations.Catelog;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategoryEntity>
{
    public void Configure(EntityTypeBuilder<ProductCategoryEntity> builder)
    {
        builder
            .ToTable("ProductCategories");

        builder
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ProductCategoryEntity.Constraints.NameMaxLength);

        builder
            .Property(x => x.IsEnabled)
            .IsRequired();

    }
}
