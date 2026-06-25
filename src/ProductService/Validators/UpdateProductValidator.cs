using FluentValidation;
using ProductService.DTOs;

namespace ProductService.Validators;

public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name must not be empty")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters");
        });

        When(x => x.Price is not null, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");
        });

        When(x => x.Stock is not null, () =>
        {
            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock must be 0 or greater");
        });
    }
}
