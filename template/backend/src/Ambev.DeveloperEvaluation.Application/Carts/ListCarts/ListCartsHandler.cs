using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

public class ListCartsQuery : IRequest<ListCartsResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Order { get; set; }
}

public class ListCartsResult
{
    public IEnumerable<CartResult> Data { get; set; } = Enumerable.Empty<CartResult>();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

public class ListCartsHandler : IRequestHandler<ListCartsQuery, ListCartsResult>
{
    private readonly ICartRepository _repo;
    private readonly IMapper _mapper;

    public ListCartsHandler(ICartRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

    public async Task<ListCartsResult> Handle(ListCartsQuery query, CancellationToken ct)
    {
        var (carts, total) = await _repo.GetPagedAsync(query.Page, query.Size, query.Order, ct);
        return new ListCartsResult
        {
            Data = _mapper.Map<IEnumerable<CartResult>>(carts),
            TotalItems = total,
            CurrentPage = query.Page,
            TotalPages = (int)Math.Ceiling((double)total / query.Size)
        };
    }
}
