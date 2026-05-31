using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCart;

public record GetCartQuery(Guid Id) : IRequest<CartResult>;

public class GetCartHandler : IRequestHandler<GetCartQuery, CartResult>
{
    private readonly ICartRepository _repo;
    private readonly IMapper _mapper;

    public GetCartHandler(ICartRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

    public async Task<CartResult> Handle(GetCartQuery query, CancellationToken ct)
    {
        var cart = await _repo.GetByIdAsync(query.Id, ct)
            ?? throw new KeyNotFoundException($"Cart with ID '{query.Id}' not found.");
        return _mapper.Map<CartResult>(cart);
    }
}
