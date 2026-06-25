using Market.Domain.Entities.Dostavljaci;

namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Create;

public sealed class CreateDostavljacCommandValidator
    : AbstractValidator<CreateDostavljacCommand>
{
    public CreateDostavljacCommandValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv je obavezno polje.")
            .MaximumLength(DostavljacEntity.Constraints.NazivMaxLength).WithMessage($"Naziv moze sadrzavati maksimalno {DostavljacEntity.Constraints.NazivMaxLength} karaktera.");
        RuleFor(x => x.Kod)
        .NotEmpty().WithMessage("Kod je obavezno polje.")
        .MaximumLength(DostavljacEntity.Constraints.KodMaxLength).WithMessage($"Kod moze sadrzavati maksimalno {DostavljacEntity.Constraints.KodMaxLength} karaktera.");
        RuleFor(x => x.Tip)
                 .IsInEnum().WithMessage("Ovaj tip nije validan.");
    }
}