using FluentValidation;

namespace Market.Application.Modules.Sales.Orders.Commands.Create;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Note cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Note));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderCommandItemValidator());

        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Order cannot contain duplicate products.")
            .When(x => x.Items != null && x.Items.Any());
    }
}

public sealed class CreateOrderCommandItemValidator : AbstractValidator<CreateOrderCommandItem>
{
    public CreateOrderCommandItemValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId must be greater than 0.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 999).WithMessage("Quantity must be between 1 and 999.");
    }
}
