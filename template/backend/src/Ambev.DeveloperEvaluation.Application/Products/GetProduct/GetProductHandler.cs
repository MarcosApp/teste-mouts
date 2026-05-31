using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProduct;

public record GetProductQuery(Guid Id) : IRequest<ProductResult>;

public class GetProductHandler : IRequestHandler<GetProductQuery, ProductResult>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public GetProductHandler(IProductRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

    public async Task<ProductResult> Handle(GetProductQuery query, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(query.Id, ct)
            ?? throw new KeyNotFoundException($"Product with ID '{query.Id}' not found.");
        return _mapper.Map<ProductResult>(product);
    }
}
