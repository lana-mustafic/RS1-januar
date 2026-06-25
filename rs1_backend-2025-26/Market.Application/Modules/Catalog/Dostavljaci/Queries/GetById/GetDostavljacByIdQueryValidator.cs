using Market.Application.Modules.Catalog.Dostavljaci.Queries.GetById;

public sealed class GetDostavljacByIdQueryValidator : AbstractValidator<GetDostavljacByIdQuery>
{
    public GetDostavljacByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id mora biti pozitivna nula.");
    }
}