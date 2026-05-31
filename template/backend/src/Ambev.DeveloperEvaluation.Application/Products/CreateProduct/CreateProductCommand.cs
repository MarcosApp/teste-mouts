using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.CreateProduct;

public class CreateProductCommand : IRequest<ProductResult>
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public RatingDto Rating { get; set; } = new();

    public ValidationResultDetail Validate()
    {
        var v = new CreateProductCommandValidator();
        var r = v.Validate(this);
        return new ValidationResultDetail { IsValid = r.IsValid, Errors = r.Errors.Select(e => (ValidationErrorDetail)e) };
    }
}

public class RatingDto
{
    public double Rate { get; set; }
    public int Count { get; set; }
}
