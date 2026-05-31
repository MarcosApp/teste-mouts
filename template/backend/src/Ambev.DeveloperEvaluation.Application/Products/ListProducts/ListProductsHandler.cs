using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProducts;

public class ListProductsQuery : IRequest<ListProductsResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Order { get; set; }
    public string? Category { get; set; }
}

public class ListProductsResult
{
    public IEnumerable<ProductResult> Data { get; set; } = Enumerable.Empty<ProductResult>();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

public class ListProductsHandler : IRequestHandler<ListProductsQuery, ListProductsResult>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public ListProductsHandler(IProductRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

    public async Task<ListProductsResult> Handle(ListProductsQuery query, CancellationToken ct)
    {
        var (products, totalCount) = await _repo.GetPagedAsync(query.Page, query.Size, query.Order, query.Category, ct);
        return new ListProductsResult
        {
            Data = _mapper.Map<IEnumerable<ProductResult>>(products),
            TotalItems = totalCount,
            CurrentPage = query.Page,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.Size)
        };
    }
}
