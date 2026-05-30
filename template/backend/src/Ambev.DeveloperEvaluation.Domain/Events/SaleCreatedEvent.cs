using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public record SaleCreatedEvent(Guid SaleId, string SaleNumber, DateTime SaleDate, Guid CustomerId, string CustomerName) : INotification;
