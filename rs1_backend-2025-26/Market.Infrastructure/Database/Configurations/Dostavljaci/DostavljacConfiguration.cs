using Market.Domain.Entities.Dostavljaci;

namespace Market.Infrastructure.Database.Configurations.Dostavljaci;

public class DostavljacConfiguration : IEntityTypeConfiguration<DostavljacEntity>
{
    public void Configure(EntityTypeBuilder<DostavljacEntity> builder)
    {
        builder
            .ToTable("Dostavljaci");

        builder
            .Property(x => x.Naziv)
            .IsRequired()
            .HasMaxLength(DostavljacEntity.Constraints.NazivMaxLength);

        builder
            .Property(x => x.Kod)
            .IsRequired()
            .HasMaxLength(DostavljacEntity.Constraints.KodMaxLength);

        builder
            .Property(x => x.Tip)
            .IsRequired();

        builder
            .Property(x => x.Aktivan)
            .IsRequired();

    }
}
