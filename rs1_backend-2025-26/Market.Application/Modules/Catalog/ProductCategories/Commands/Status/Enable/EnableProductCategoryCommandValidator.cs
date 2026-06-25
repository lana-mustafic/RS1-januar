namespace Market.Application.Modules.Catalog.ProductCategories.Commands.Status.Enable;

public sealed class EnableProductCategoryCommandValidator : AbstractValidator<EnableProductCategoryCommand>
{
    public EnableProductCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
