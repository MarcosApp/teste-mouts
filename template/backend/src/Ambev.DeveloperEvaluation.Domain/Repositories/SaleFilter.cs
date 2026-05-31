namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public class SaleFilter
{
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? BranchId { get; set; }
    public string? SaleNumber { get; set; }
    public bool? IsCancelled { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }
}
