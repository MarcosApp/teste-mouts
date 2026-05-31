namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartProduct> Products { get; set; } = new();

    public Cart()
    {
        Id = Guid.NewGuid();
        Date = DateTime.UtcNow;
    }
}

public class CartProduct
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
