namespace Market.Application.Modules.Catalog.ProductCategories.Commands.Create;

public sealed class CreateProductCategoryCommandValidator
    : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(ProductCategoryEntity.Constraints.NameMaxLength).WithMessage($"Name can be at most {ProductCategoryEntity.Constraints.NameMaxLength} characters long.");
    }
}