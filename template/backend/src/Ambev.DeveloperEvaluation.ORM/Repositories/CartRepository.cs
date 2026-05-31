using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class CartRepository : ICartRepository
{
    private readonly MongoDbContext _context;

    public CartRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await _context.Carts.InsertOneAsync(cart, cancellationToken: cancellationToken);
        return cart;
    }

    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Cart>.Filter.Eq(c => c.Id, id);
        return await _context.Carts.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Cart> UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Cart>.Filter.Eq(c => c.Id, cart.Id);
        await _context.Carts.ReplaceOneAsync(filter, cart, cancellationToken: cancellationToken);
        return cart;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Cart>.Filter.Eq(c => c.Id, id);
        var result = await _context.Carts.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<(IEnumerable<Cart> Carts, int TotalCount)> GetPagedAsync(
        int page, int size, string? orderBy, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Cart>.Filter.Empty;
        var total = (int)await _context.Carts.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var sort = orderBy?.ToLower() switch
        {
            "date desc" => Builders<Cart>.Sort.Descending(c => c.Date),
            "date" or "date asc" => Builders<Cart>.Sort.Ascending(c => c.Date),
            _ => Builders<Cart>.Sort.Descending(c => c.Date)
        };

        var carts = await _context.Carts
            .Find(filter)
            .Sort(sort)
            .Skip((page - 1) * size)
            .Limit(size)
            .ToListAsync(cancellationToken);

        return (carts, total);
    }
}
