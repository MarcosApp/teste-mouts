using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesQuery, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<ListSalesResult> Handle(ListSalesQuery query, CancellationToken cancellationToken)
    {
        var (sales, totalCount) = await _saleRepository.GetPagedAsync(query.Page, query.Size, query.Order, cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / query.Size);

        return new ListSalesResult
        {
            Sales = _mapper.Map<IEnumerable<GetSaleResult>>(sales),
            TotalCount = totalCount,
            CurrentPage = query.Page,
            TotalPages = totalPages
        };
    }
}
