using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;

    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(SaleItem item)
    {
        item.Sale = this;
        _items.Add(item);
        RecalculateTotal();
    }

    public void SetItems(IEnumerable<SaleItem> items)
    {
        _items.Clear();
        foreach (var item in items)
        {
            item.Sale = this;
            _items.Add(item);
        }
        RecalculateTotal();
    }

    public void Cancel()
    {
        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        IsCancelled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecalculateTotal()
    {
        TotalAmount = _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);
    }

    public void UpdatedNow()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}
