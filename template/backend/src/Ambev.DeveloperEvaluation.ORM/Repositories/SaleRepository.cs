using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale == null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPagedAsync(
        int page, int size, string? orderBy,
        SaleFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.Include(s => s.Items).AsQueryable();

        if (filter != null)
            query = ApplyFilter(query, filter);

        if (!string.IsNullOrWhiteSpace(orderBy))
            query = ApplyOrdering(query, orderBy);
        else
            query = query.OrderByDescending(s => s.SaleDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var sales = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (sales, totalCount);
    }

    private static IQueryable<Sale> ApplyFilter(IQueryable<Sale> query, SaleFilter filter)
    {
        if (filter.CustomerId.HasValue)
            query = query.Where(s => s.CustomerId == filter.CustomerId.Value);

        if (!string.IsNullOrWhiteSpace(filter.CustomerName))
        {
            var name = filter.CustomerName.Trim('*').ToLower();
            if (filter.CustomerName.StartsWith('*') && filter.CustomerName.EndsWith('*'))
                query = query.Where(s => s.CustomerName.ToLower().Contains(name));
            else if (filter.CustomerName.StartsWith('*'))
                query = query.Where(s => s.CustomerName.ToLower().EndsWith(name));
            else if (filter.CustomerName.EndsWith('*'))
                query = query.Where(s => s.CustomerName.ToLower().StartsWith(name));
            else
                query = query.Where(s => s.CustomerName.ToLower() == name);
        }

        if (filter.BranchId.HasValue)
            query = query.Where(s => s.BranchId == filter.BranchId.Value);

        if (!string.IsNullOrWhiteSpace(filter.SaleNumber))
        {
            var num = filter.SaleNumber.Trim('*').ToLower();
            if (filter.SaleNumber.EndsWith('*'))
                query = query.Where(s => s.SaleNumber.ToLower().StartsWith(num));
            else
                query = query.Where(s => s.SaleNumber.ToLower() == num);
        }

        if (filter.IsCancelled.HasValue)
            query = query.Where(s => s.IsCancelled == filter.IsCancelled.Value);

        if (filter.MinDate.HasValue)
            query = query.Where(s => s.SaleDate >= filter.MinDate.Value);

        if (filter.MaxDate.HasValue)
            query = query.Where(s => s.SaleDate <= filter.MaxDate.Value);

        if (filter.MinTotalAmount.HasValue)
            query = query.Where(s => s.TotalAmount >= filter.MinTotalAmount.Value);

        if (filter.MaxTotalAmount.HasValue)
            query = query.Where(s => s.TotalAmount <= filter.MaxTotalAmount.Value);

        return query;
    }

    private static IQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string orderBy)
    {
        var parts = orderBy.Trim('"').Split(',');
        IOrderedQueryable<Sale>? ordered = null;

        foreach (var part in parts)
        {
            var tokens = part.Trim().Split(' ');
            var field = tokens[0].Trim();
            var desc = tokens.Length > 1 && tokens[1].Trim().ToLower() == "desc";

            ordered = (field.ToLower(), desc, ordered) switch
            {
                ("salenumber", false, null)    => query.OrderBy(s => s.SaleNumber),
                ("salenumber", true, null)     => query.OrderByDescending(s => s.SaleNumber),
                ("saledate", false, null)      => query.OrderBy(s => s.SaleDate),
                ("saledate", true, null)       => query.OrderByDescending(s => s.SaleDate),
                ("customername", false, null)  => query.OrderBy(s => s.CustomerName),
                ("customername", true, null)   => query.OrderByDescending(s => s.CustomerName),
                ("totalamount", false, null)   => query.OrderBy(s => s.TotalAmount),
                ("totalamount", true, null)    => query.OrderByDescending(s => s.TotalAmount),
                ("salenumber", false, not null)   => ordered!.ThenBy(s => s.SaleNumber),
                ("salenumber", true, not null)    => ordered!.ThenByDescending(s => s.SaleNumber),
                ("saledate", false, not null)     => ordered!.ThenBy(s => s.SaleDate),
                ("saledate", true, not null)      => ordered!.ThenByDescending(s => s.SaleDate),
                ("customername", false, not null) => ordered!.ThenBy(s => s.CustomerName),
                ("customername", true, not null)  => ordered!.ThenByDescending(s => s.CustomerName),
                ("totalamount", false, not null)  => ordered!.ThenBy(s => s.TotalAmount),
                ("totalamount", true, not null)   => ordered!.ThenByDescending(s => s.TotalAmount),
                _ => ordered ?? query.OrderByDescending(s => s.SaleDate)
            };
        }

        return ordered ?? query.OrderByDescending(s => s.SaleDate);
    }
}
