namespace Market.Infrastructure.Database.Configurations.Catalog;

public sealed class PromotionConfiguration : IEntityTypeConfiguration<PromotionEntity>
{
    public void Configure(EntityTypeBuilder<PromotionEntity> builder)
    {
        builder.ToTable("Promotions");

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(PromotionEntity.Constraints.TitleMaxLength);

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(PromotionEntity.Constraints.ImageUrlMaxLength);

        builder.Property(x => x.TargetUrl)
            .HasMaxLength(PromotionEntity.Constraints.TargetUrlMaxLength);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        // Index for efficient querying of active promotions
        builder.HasIndex(x => new { x.IsActive, x.SortOrder })
            .HasDatabaseName("IX_Promotions_IsActive_SortOrder");
    }
}
