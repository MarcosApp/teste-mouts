using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact(DisplayName = "Given valid sale data When adding items Then total amount is correctly calculated")]
    public void AddItem_ValidItems_CorrectTotalAmount()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(0);
        var item1 = SaleTestData.GenerateValidItem(quantity: 3, unitPrice: 100m);
        var item2 = SaleTestData.GenerateValidItem(quantity: 2, unitPrice: 50m);

        // When
        sale.AddItem(item1);
        sale.AddItem(item2);

        // Then
        var expected = (3 * 100m) + (2 * 50m);
        sale.TotalAmount.Should().Be(expected);
        sale.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Given sale with items When item is cancelled Then total excludes cancelled item")]
    public void RecalculateTotal_WithCancelledItem_ExcludesItFromTotal()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(0);
        var item1 = SaleTestData.GenerateValidItem(quantity: 3, unitPrice: 100m);
        var item2 = SaleTestData.GenerateValidItem(quantity: 2, unitPrice: 50m);
        sale.AddItem(item1);
        sale.AddItem(item2);

        // When
        item2.Cancel();

        // Then
        sale.TotalAmount.Should().Be(3 * 100m);
    }

    [Fact(DisplayName = "Given active sale When cancelled Then IsCancelled is true")]
    public void Cancel_ActiveSale_SetsIsCancelledTrue()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale();

        // When
        sale.Cancel();

        // Then
        sale.IsCancelled.Should().BeTrue();
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given valid sale When SetItems called Then items are replaced and total recalculated")]
    public void SetItems_NewItems_ReplacesExistingAndRecalculatesTotal()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var newItem = SaleTestData.GenerateValidItem(quantity: 5, unitPrice: 20m);

        // When
        sale.SetItems(new[] { newItem });

        // Then
        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().Be(5 * 20m * 0.90m); // 4+ items = 10% discount
    }

    [Fact(DisplayName = "Given new sale When created Then IsCancelled is false and has CreatedAt")]
    public void NewSale_DefaultState_IsNotCancelledAndHasCreatedAt()
    {
        // Given / When
        var sale = new Sale();

        // Then
        sale.IsCancelled.Should().BeFalse();
        sale.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "Given sale When validated with valid data Then validation passes")]
    public void Validate_ValidSale_ReturnsIsValid()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(1);

        // When
        var result = sale.Validate();

        // Then
        result.IsValid.Should().BeTrue();
    }
}
