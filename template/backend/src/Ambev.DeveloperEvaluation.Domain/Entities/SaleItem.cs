using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    private const int MaxQuantityPerProduct = 20;
    private const int MinQuantityForDiscount = 4;
    private const int MinQuantityForHighDiscount = 10;
    private const decimal LowDiscountRate = 0.10m;
    private const decimal HighDiscountRate = 0.20m;

    public Guid SaleId { get; set; }
    public Sale? Sale { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }

    public SaleItem() { }

    public SaleItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        SetQuantity(quantity);
    }

    public void SetQuantity(int quantity)
    {
        if (quantity > MaxQuantityPerProduct)
            throw new DomainException($"Cannot sell more than {MaxQuantityPerProduct} identical items.");

        Quantity = quantity;
        Discount = CalculateDiscount(quantity);
        TotalAmount = (UnitPrice * quantity) * (1 - Discount);
    }

    public void Cancel()
    {
        IsCancelled = true;
        Sale?.RecalculateTotal();
    }

    private static decimal CalculateDiscount(int quantity)
    {
        if (quantity >= MinQuantityForHighDiscount)
            return HighDiscountRate;

        if (quantity >= MinQuantityForDiscount)
            return LowDiscountRate;

        return 0m;
    }
}
