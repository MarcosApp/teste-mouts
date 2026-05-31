using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Application.Products.ListProducts;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Creates a new product.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<ProductResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var command = new CreateProductCommand
        {
            Title = request.Title,
            Price = request.Price,
            Description = request.Description,
            Category = request.Category,
            Image = request.Image,
            Rating = new RatingDto { Rate = request.Rating.Rate, Count = request.Rating.Count }
        };

        var result = await _mediator.Send(command, ct);
        return ApiCreated($"/api/products/{result.Id}", new ApiResponseWithData<ProductResult>
        {
            Success = true,
            Message = "Product created successfully",
            Data = result
        });
    }

    /// <summary>Retrieves a paginated list of products.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseWithData<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int _page = 1,
        [FromQuery] int _size = 10,
        [FromQuery] string? _order = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListProductsQuery { Page = _page, Size = _size, Order = _order }, ct);
        return ApiOk(new
        {
            data = result.Data,
            totalItems = result.TotalItems,
            currentPage = result.CurrentPage,
            totalPages = result.TotalPages
        });
    }

    /// <summary>Retrieves all product categories.</summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponseWithData<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), ct);
        return ApiOk(new ApiResponseWithData<IEnumerable<string>>
        {
            Success = true,
            Message = "Categories retrieved successfully",
            Data = result
        });
    }

    /// <summary>Retrieves products filtered by category.</summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponseWithData<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(
        [FromRoute] string category,
        [FromQuery] int _page = 1,
        [FromQuery] int _size = 10,
        [FromQuery] string? _order = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListProductsQuery { Page = _page, Size = _size, Order = _order, Category = category }, ct);
        return ApiOk(new
        {
            data = result.Data,
            totalItems = result.TotalItems,
            currentPage = result.CurrentPage,
            totalPages = result.TotalPages
        });
    }

    /// <summary>Retrieves a product by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<ProductResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductQuery(id), ct);
        return ApiOk(new ApiResponseWithData<ProductResult>
        {
            Success = true,
            Message = "Product retrieved successfully",
            Data = result
        });
    }

    /// <summary>Updates an existing product.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<ProductResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var command = new UpdateProductCommand
        {
            Id = id,
            Title = request.Title,
            Price = request.Price,
            Description = request.Description,
            Category = request.Category,
            Image = request.Image,
            Rating = new RatingDto { Rate = request.Rating.Rate, Count = request.Rating.Count }
        };

        var result = await _mediator.Send(command, ct);
        return ApiOk(new ApiResponseWithData<ProductResult>
        {
            Success = true,
            Message = "Product updated successfully",
            Data = result
        });
    }

    /// <summary>Deletes a product by ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProductCommand(id), ct);
        return ApiOk(new ApiResponse { Success = true, Message = "Product deleted successfully" });
    }
}
