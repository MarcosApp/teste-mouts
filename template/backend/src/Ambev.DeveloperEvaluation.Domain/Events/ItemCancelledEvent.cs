using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public record ItemCancelledEvent(Guid SaleId, Guid ItemId, Guid ProductId, string ProductName) : INotification;
