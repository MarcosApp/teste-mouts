using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DefaultContext _context;

    public ProductRepository(DefaultContext context) { _context = context; }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetByIdAsync(id, cancellationToken);
        if (product == null) return false;
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
        int page, int size, string? orderBy, string? category = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category.ToLower() == category.ToLower());

        query = orderBy?.Trim('"').ToLower() switch
        {
            "price desc" => query.OrderByDescending(p => p.Price),
            "price" or "price asc" => query.OrderBy(p => p.Price),
            "title desc" => query.OrderByDescending(p => p.Title),
            _ => query.OrderBy(p => p.Title)
        };

        var total = await query.CountAsync(cancellationToken);
        var products = await query.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
        return (products, total);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => await _context.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync(cancellationToken);
}
