using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;

public record DeleteCartCommand(Guid Id) : IRequest<bool>;

public class DeleteCartHandler : IRequestHandler<DeleteCartCommand, bool>
{
    private readonly ICartRepository _repo;

    public DeleteCartHandler(ICartRepository repo) { _repo = repo; }

    public async Task<bool> Handle(DeleteCartCommand command, CancellationToken ct)
    {
        var cart = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Cart with ID '{command.Id}' not found.");
        return await _repo.DeleteAsync(command.Id, ct);
    }
}
