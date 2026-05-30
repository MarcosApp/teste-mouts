using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Sale ID is required.");

        RuleFor(x => x.SaleNumber)
            .NotEmpty().WithMessage("Sale number is required.")
            .MaximumLength(50);

        RuleFor(x => x.SaleDate).NotEmpty().WithMessage("Sale date is required.");

        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(100);

        RuleFor(x => x.BranchId).NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("Branch name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Items).NotEmpty().WithMessage("Sale must have at least one item.");

        RuleForEach(x => x.Items).SetValidator(new CreateSaleItemCommandValidator());
    }
}

public class CreateSaleItemCommandValidator : AbstractValidator<CreateSaleItemCommand>
{
    public CreateSaleItemCommandValidator()
    {
        RuleFor(i => i.ProductId).NotEmpty().WithMessage("Product ID is required.");
        RuleFor(i => i.ProductName).NotEmpty().WithMessage("Product name is required.");
        RuleFor(i => i.Quantity).GreaterThan(0).LessThanOrEqualTo(20).WithMessage("Quantity must be between 1 and 20.");
        RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage("Unit price must be greater than zero.");
    }
}
