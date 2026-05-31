using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleItemTests
{
    [Theory(DisplayName = "Given quantity below 4 When creating item Then no discount is applied")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void SetQuantity_BelowMinimumForDiscount_NoDiscount(int quantity)
    {
        // Given / When
        var item = new SaleItem(Guid.NewGuid(), "Product", quantity, 100m);

        // Then
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(quantity * 100m);
    }

    [Theory(DisplayName = "Given quantity between 4 and 9 When creating item Then 10% discount is applied")]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(9)]
    public void SetQuantity_Between4And9_TenPercentDiscount(int quantity)
    {
        // Given / When
        var item = new SaleItem(Guid.NewGuid(), "Product", quantity, 100m);

        // Then
        item.Discount.Should().Be(0.10m);
        item.TotalAmount.Should().Be(quantity * 100m * 0.90m);
    }

    [Theory(DisplayName = "Given quantity between 10 and 20 When creating item Then 20% discount is applied")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void SetQuantity_Between10And20_TwentyPercentDiscount(int quantity)
    {
        // Given / When
        var item = new SaleItem(Guid.NewGuid(), "Product", quantity, 100m);

        // Then
        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(quantity * 100m * 0.80m);
    }

    [Theory(DisplayName = "Given quantity above 20 When creating item Then throws DomainException")]
    [InlineData(21)]
    [InlineData(50)]
    [InlineData(100)]
    public void SetQuantity_Above20_ThrowsDomainException(int quantity)
    {
        // Given / When
        var act = () => new SaleItem(Guid.NewGuid(), "Product", quantity, 100m);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("*20*");
    }

    [Fact(DisplayName = "Given valid item When cancelled Then IsCancelled is true")]
    public void Cancel_ValidItem_SetsIsCancelledTrue()
    {
        // Given
        var item = new SaleItem(Guid.NewGuid(), "Product", 3, 50m);

        // When
        item.Cancel();

        // Then
        item.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Given item with quantity 4 When total calculated Then discounted total is correct")]
    public void TotalAmount_Quantity4_CorrectlyDiscounted()
    {
        // Given / When
        var item = new SaleItem(Guid.NewGuid(), "Product", 4, 50m);

        // Then
        item.TotalAmount.Should().Be(4 * 50m * 0.90m);
    }
}
