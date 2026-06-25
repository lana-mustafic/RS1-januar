using FluentValidation;
using Market.Domain.Entities.Sales;

namespace Market.Application.Modules.Sales.Orders.Commands.Status;

public sealed class ChangeOrderStatusCommandValidator : AbstractValidator<ChangeOrderStatusCommand>
{
    public ChangeOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Order Id must be greater than 0.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid order status.");
    }
}
