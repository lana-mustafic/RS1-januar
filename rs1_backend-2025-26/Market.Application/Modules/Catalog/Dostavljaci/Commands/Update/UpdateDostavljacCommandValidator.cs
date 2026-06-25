using Market.Domain.Entities.Dostavljaci;

namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Update;

public sealed class UpdateDostavljacCommandValidator
    : AbstractValidator<UpdateDostavljacCommand>
{
    public UpdateDostavljacCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv je obavezno polje.")
            .MaximumLength(DostavljacEntity.Constraints.NazivMaxLength).WithMessage($"Naziv moze imati maksimalno {DostavljacEntity.Constraints.NazivMaxLength} karaktera.");
        RuleFor(x => x.Kod)
            .NotEmpty().WithMessage("Kod je obavezno polje.")
            .MaximumLength(DostavljacEntity.Constraints.KodMaxLength).WithMessage($"Kod moze imati maksimalno {DostavljacEntity.Constraints.KodMaxLength} karaktera.");
        RuleFor(x => x.Tip)
            .IsInEnum();
    }
}