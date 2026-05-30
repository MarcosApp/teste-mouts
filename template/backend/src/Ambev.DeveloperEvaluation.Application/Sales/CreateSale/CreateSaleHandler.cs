using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateSaleHandler> _logger;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IMediator mediator, ILogger<CreateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingSale = await _saleRepository.GetBySaleNumberAsync(command.SaleNumber, cancellationToken);
        if (existingSale != null)
            throw new InvalidOperationException($"Sale with number '{command.SaleNumber}' already exists.");

        var sale = _mapper.Map<Sale>(command);

        foreach (var itemCmd in command.Items)
        {
            var item = new SaleItem(itemCmd.ProductId, itemCmd.ProductName, itemCmd.Quantity, itemCmd.UnitPrice);
            sale.AddItem(item);
        }

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);

        _logger.LogInformation("SaleCreated: SaleId={SaleId}, SaleNumber={SaleNumber}", created.Id, created.SaleNumber);
        await _mediator.Publish(new SaleCreatedEvent(created.Id, created.SaleNumber, created.SaleDate, created.CustomerId, created.CustomerName), cancellationToken);

        return _mapper.Map<CreateSaleResult>(created);
    }
}
