using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;

public class UpdateProductCommand : IRequest<ProductResult>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public RatingDto Rating { get; set; } = new();
}

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductResult>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public UpdateProductHandler(IProductRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

    public async Task<ProductResult> Handle(UpdateProductCommand command, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Product with ID '{command.Id}' not found.");

        product.Title = command.Title;
        product.Price = command.Price;
        product.Description = command.Description;
        product.Category = command.Category;
        product.Image = command.Image;
        product.Rating = new Rating(command.Rating.Rate, command.Rating.Count);
        product.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateAsync(product, ct);
        return _mapper.Map<ProductResult>(updated);
    }
}
