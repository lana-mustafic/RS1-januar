namespace Market.Application.Modules.Catalog.ProductCategories.Commands.Status.Disable;

public sealed class DisableProductCategoryCommandValidator : AbstractValidator<DisableProductCategoryCommand>
{
    public DisableProductCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
