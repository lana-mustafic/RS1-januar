using Market.Domain.Common;

namespace Market.Domain.Entities.Fakture;

/// <summary>
/// Predstavlja fakturu u sistemu.
/// </summary>
public class FakturaEntity : BaseEntity
{
    /// <summary>
    /// Broj računa (human-readable, npr. FAK-2026-0001).
    /// </summary>
    public required string BrojRacuna { get; set; }

    /// <summary>
    /// Tip fakture (ulazna/izlazna).
    /// </summary>
    public required FakturaTip Tip { get; set; }

    /// <summary>
    /// Napomena (opcionalno polje).
    /// </summary>
    public string? Napomena { get; set; }

    /// <summary>
    /// Broj stavki u fakturi.
    /// </summary>
    public int BrojStavki { get; set; }

    /// <summary>
    /// Single source of truth for technical/business constraints.
    /// Used in validators and EF configuration.
    /// </summary>
    public static class Constraints
    {
        public const int BrojRacunaMaxLength = 20;
        public const int NapomenaMaxLength = 500;
    }
}
