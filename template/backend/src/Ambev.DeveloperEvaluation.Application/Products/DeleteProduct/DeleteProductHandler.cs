using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _repo;

    public DeleteProductHandler(IProductRepository repo) { _repo = repo; }

    public async Task<bool> Handle(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Product with ID '{command.Id}' not found.");

        return await _repo.DeleteAsync(command.Id, ct);
    }
}
