using MediatR;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<CancelSaleItemHandler> _logger;

    public CancelSaleItemHandler(ISaleRepository saleRepository, IMediator mediator, ILogger<CancelSaleItemHandler> logger)
    {
        _saleRepository = saleRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID '{command.SaleId}' not found.");

        var item = sale.Items.FirstOrDefault(i => i.Id == command.ItemId);
        if (item == null)
            throw new KeyNotFoundException($"Item with ID '{command.ItemId}' not found in sale.");

        if (item.IsCancelled)
            throw new InvalidOperationException("Item is already cancelled.");

        item.Cancel();
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        _logger.LogInformation("ItemCancelled: SaleId={SaleId}, ItemId={ItemId}, ProductName={ProductName}", sale.Id, item.Id, item.ProductName);
        await _mediator.Publish(new ItemCancelledEvent(sale.Id, item.Id, item.ProductId, item.ProductName), cancellationToken);

        return new CancelSaleItemResult { SaleId = sale.Id, ItemId = item.Id, IsCancelled = item.IsCancelled };
    }
}
