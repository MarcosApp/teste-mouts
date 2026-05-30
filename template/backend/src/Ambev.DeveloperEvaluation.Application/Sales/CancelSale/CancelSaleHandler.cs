using MediatR;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<CancelSaleHandler> _logger;

    public CancelSaleHandler(ISaleRepository saleRepository, IMediator mediator, ILogger<CancelSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CancelSaleResult> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID '{command.Id}' not found.");

        if (sale.IsCancelled)
            throw new InvalidOperationException($"Sale '{sale.SaleNumber}' is already cancelled.");

        sale.Cancel();
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        _logger.LogInformation("SaleCancelled: SaleId={SaleId}, SaleNumber={SaleNumber}", sale.Id, sale.SaleNumber);
        await _mediator.Publish(new SaleCancelledEvent(sale.Id, sale.SaleNumber), cancellationToken);

        return new CancelSaleResult { Id = sale.Id, SaleNumber = sale.SaleNumber, IsCancelled = sale.IsCancelled };
    }
}
