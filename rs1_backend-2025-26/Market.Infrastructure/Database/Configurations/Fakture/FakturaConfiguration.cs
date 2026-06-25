using Market.Domain.Entities.Fakture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Market.Infrastructure.Database.Configurations.Fakture;

public class FakturaConfiguration : IEntityTypeConfiguration<FakturaEntity>
{
    public void Configure(EntityTypeBuilder<FakturaEntity> builder)
    {
        builder
            .ToTable("Fakture");

        builder
            .Property(x => x.BrojRacuna)
            .HasMaxLength(FakturaEntity.Constraints.BrojRacunaMaxLength)
            .IsRequired();

        builder
            .Property(x => x.Tip)
            .IsRequired();

        builder
            .Property(x => x.Napomena)
            .HasMaxLength(FakturaEntity.Constraints.NapomenaMaxLength);
    }
}
