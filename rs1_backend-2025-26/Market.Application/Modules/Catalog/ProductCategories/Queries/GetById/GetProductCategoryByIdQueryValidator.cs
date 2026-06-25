using Market.Application.Modules.Catalog.ProductCategories.Queries.GetById;

public sealed class GetProductCategoryByIdQueryValidator : AbstractValidator<GetProductCategoryByIdQuery>
{
    public GetProductCategoryByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be a positive value.");
    }
}