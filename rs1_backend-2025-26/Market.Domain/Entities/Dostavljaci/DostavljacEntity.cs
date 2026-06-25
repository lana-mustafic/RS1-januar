using Market.Domain.Common;

namespace Market.Domain.Entities.Dostavljaci;
public class DostavljacEntity : BaseEntity
{
    public required string Naziv { get; set; }

    public required string Kod { get; set; }

    public required DostavljacTip Tip { get; set; }

    public bool Aktivan { get; set; }

    public static class Constraints
    {
        public const int NazivMaxLength = 100;
        public const int KodMaxLength = 3;
    }
}
