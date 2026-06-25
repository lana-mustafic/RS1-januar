namespace Market.Application.Modules.Catalog.CatalogHome.Queries.GetCatalogHome;

public sealed class GetCatalogHomeQuery : IRequest<CatalogHomeDto>
{
    public int CategoryLimit { get; set; } = 8;
    public int ProductLimit { get; set; } = 12;
}

public sealed class CatalogHomeDto
{
    public List<CatalogCategoryDto> Categories { get; set; } = new();
    public List<CatalogProductDto> NewestProducts { get; set; } = new();
    public List<CatalogPromotionDto> Promotions { get; set; } = new();
    public CatalogHomeStatsDto Stats { get; set; } = new();
}

public sealed class CatalogCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class CatalogProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public sealed class CatalogPromotionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public int SortOrder { get; set; }
}

public sealed class CatalogHomeStatsDto
{
    public int TotalCategories { get; set; }
    public int TotalProducts { get; set; }
}
