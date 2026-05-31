using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleTestData
{
    private static readonly Faker<SaleItem> SaleItemFaker = new Faker<SaleItem>()
        .CustomInstantiator(f => new SaleItem(
            Guid.NewGuid(),
            f.Commerce.ProductName(),
            f.Random.Int(1, 3),
            f.Random.Decimal(5, 100)));

    private static readonly Faker<Sale> SaleFaker = new Faker<Sale>()
        .RuleFor(s => s.Id, _ => Guid.NewGuid())
        .RuleFor(s => s.SaleNumber, f => f.Random.AlphaNumeric(8).ToUpper())
        .RuleFor(s => s.SaleDate, f => f.Date.Recent(30))
        .RuleFor(s => s.CustomerId, _ => Guid.NewGuid())
        .RuleFor(s => s.CustomerName, f => f.Person.FullName)
        .RuleFor(s => s.BranchId, _ => Guid.NewGuid())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName());

    public static Sale GenerateValidSale(int itemCount = 1)
    {
        var sale = SaleFaker.Generate();
        for (var i = 0; i < itemCount; i++)
            sale.AddItem(SaleItemFaker.Generate());
        return sale;
    }

    public static SaleItem GenerateValidItem(int quantity = 1, decimal unitPrice = 10m)
        => new SaleItem(Guid.NewGuid(), "Test Product", quantity, unitPrice);

    public static CreateSaleCommand GenerateValidCreateCommand()
    {
        var faker = new Faker();
        return new CreateSaleCommand
        {
            SaleNumber = faker.Random.AlphaNumeric(8).ToUpper(),
            SaleDate = faker.Date.Recent(30),
            CustomerId = Guid.NewGuid(),
            CustomerName = faker.Person.FullName,
            BranchId = Guid.NewGuid(),
            BranchName = faker.Company.CompanyName(),
            Items = new List<CreateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = faker.Commerce.ProductName(),
                    Quantity = 3,
                    UnitPrice = faker.Random.Decimal(5, 100)
                }
            }
        };
    }
}
