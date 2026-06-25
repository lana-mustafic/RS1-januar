namespace Market.Infrastructure.Database.Configurations.Catalog;

public class UserProductFavoriteConfiguration : IEntityTypeConfiguration<UserProductFavoriteEntity>
{
    public void Configure(EntityTypeBuilder<UserProductFavoriteEntity> builder)
    {
        builder
            .ToTable("UserProductFavorites");

        // RS1 upute: KOMPOZITNI UNIQUE INDEX - sprečava duplikate (UserId, ProductId)
        // Ovo je alternativa za kompozitni primarni ključ a koja radi sa BaseEntity koji ima Id kao PK
        builder.HasIndex(x => new { x.UserId, x.ProductId })
            .IsUnique();

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(x => x.FavoriteProducts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade); // RS1 upiute: Ako se user obriše, obrišu se i favoriti

        builder.HasOne(x => x.Product)
            .WithMany(x => x.FavoritedByUsers)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade); // RS1 upiute: Ako se product obriše, obrišu se i favoriti
    }
}