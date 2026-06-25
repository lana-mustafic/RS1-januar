namespace Market.Domain.Entities.Fakture;

/// <summary>
/// Definiše tipove fakture.
/// </summary>
public enum FakturaTip
{
    /// <summary>
    /// Ulazna faktura - unos robe / povećanje zaliha.
    /// </summary>
    Ulazna = 1,

    /// <summary>
    /// Izlazna faktura - iznos robe / smanjenje zaliha.
    /// </summary>
    Izlazna = 2
}
