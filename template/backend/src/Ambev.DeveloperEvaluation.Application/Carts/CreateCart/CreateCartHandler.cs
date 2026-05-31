using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

public class CreateCartHandler : IRequestHandler<CreateCartCommand, CartResult>
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;

    public CreateCartHandler(ICartRepository cartRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _mapper = mapper;
    }

    public async Task<CartResult> Handle(CreateCartCommand command, CancellationToken cancellationToken)
    {
        if (command.UserId == Guid.Empty)
            throw new ValidationException("UserId is required.");

        if (!command.Products.Any())
            throw new ValidationException("Cart must have at least one product.");

        var cart = new Cart
        {
            UserId = command.UserId,
            Date = command.Date == default ? DateTime.UtcNow : command.Date,
            Products = command.Products.Select(p => new CartProduct
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity
            }).ToList()
        };

        var created = await _cartRepository.CreateAsync(cart, cancellationToken);
        return _mapper.Map<CartResult>(created);
    }
}
