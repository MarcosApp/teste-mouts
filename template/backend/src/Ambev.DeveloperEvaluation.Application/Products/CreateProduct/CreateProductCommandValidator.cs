using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Products.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than zero.");
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Rating.Rate).InclusiveBetween(0, 5).WithMessage("Rate must be between 0 and 5.");
        RuleFor(x => x.Rating.Count).GreaterThanOrEqualTo(0);
    }
}
