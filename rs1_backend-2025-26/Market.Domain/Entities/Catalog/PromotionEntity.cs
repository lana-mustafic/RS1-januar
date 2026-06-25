using Market.Domain.Common;

namespace Market.Domain.Entities.Catalog;

public sealed class PromotionEntity : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public int SortOrder { get; set; }

    public static class Constraints
    {
        public const int TitleMaxLength = 120;
        public const int ImageUrlMaxLength = 500;
        public const int TargetUrlMaxLength = 500;
    }
}
