namespace Market.Application.Modules.Catalog.ProductCategories.Commands.Update;

public sealed class UpdateProductCategoryCommandValidator
    : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(ProductCategoryEntity.Constraints.NameMaxLength).WithMessage($"Name can be at most {ProductCategoryEntity.Constraints.NameMaxLength} characters long.");
    }
}