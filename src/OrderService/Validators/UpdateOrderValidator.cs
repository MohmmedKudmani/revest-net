using FluentValidation;
using OrderService.DTOs;

namespace OrderService.Validators;

public class UpdateOrderValidator : AbstractValidator<UpdateOrderDto>
{
    private static readonly string[] AllowedStatuses = ["PENDING", "CONFIRMED", "CANCELLED"];

    public UpdateOrderValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage("Status must be one of: PENDING, CONFIRMED, CANCELLED");
    }
}
