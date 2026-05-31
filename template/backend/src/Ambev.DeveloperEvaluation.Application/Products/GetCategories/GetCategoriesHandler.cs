using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProduct;

public record GetCategoriesQuery : IRequest<IEnumerable<string>>;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<string>>
{
    private readonly IProductRepository _repo;

    public GetCategoriesHandler(IProductRepository repo) { _repo = repo; }

    public async Task<IEnumerable<string>> Handle(GetCategoriesQuery query, CancellationToken ct)
        => await _repo.GetCategoriesAsync(ct);
}
