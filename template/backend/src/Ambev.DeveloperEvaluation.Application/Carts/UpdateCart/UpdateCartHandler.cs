using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

public class UpdateCartCommand : IRequest<CartResult>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartProductCommand> Products { get; set; } = new();
}

public class UpdateCartHandler : IRequestHandler<UpdateCartCommand, CartResult>
{
    private readonly ICartRepository _repo;
    private readonly IMapper _mapper;

    public UpdateCartHandler(ICartRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

    public async Task<CartResult> Handle(UpdateCartCommand command, CancellationToken ct)
    {
        var cart = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Cart with ID '{command.Id}' not found.");

        cart.UserId = command.UserId;
        cart.Date = command.Date;
        cart.Products = command.Products.Select(p => new CartProduct
        {
            ProductId = p.ProductId,
            Quantity = p.Quantity
        }).ToList();

        var updated = await _repo.UpdateAsync(cart, ct);
        return _mapper.Map<CartResult>(updated);
    }
}
