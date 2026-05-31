using Ambev.DeveloperEvaluation.Application.Carts;
using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCart;
using Ambev.DeveloperEvaluation.Application.Carts.ListCarts;
using Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;
using Ambev.DeveloperEvaluation.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts;

[ApiController]
[Route("api/[controller]")]
public class CartsController : BaseController
{
    private readonly IMediator _mediator;

    public CartsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Creates a new cart.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CartResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCart([FromBody] CreateCartCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return ApiCreated($"/api/carts/{result.Id}", new ApiResponseWithData<CartResult>
        {
            Success = true,
            Message = "Cart created successfully",
            Data = result
        });
    }

    /// <summary>Retrieves a paginated list of carts.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCarts(
        [FromQuery] int _page = 1,
        [FromQuery] int _size = 10,
        [FromQuery] string? _order = null,
        [FromQuery] Guid? userId = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListCartsQuery { Page = _page, Size = _size, Order = _order, UserId = userId }, ct);
        return ApiOk(new
        {
            data = result.Data,
            totalItems = result.TotalItems,
            currentPage = result.CurrentPage,
            totalPages = result.TotalPages
        });
    }

    /// <summary>Retrieves a cart by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<CartResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCart([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCartQuery(id), ct);
        return ApiOk(new ApiResponseWithData<CartResult>
        {
            Success = true,
            Message = "Cart retrieved successfully",
            Data = result
        });
    }

    /// <summary>Updates an existing cart.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<CartResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCart([FromRoute] Guid id, [FromBody] UpdateCartCommand command, CancellationToken ct)
    {
        command.Id = id;
        var result = await _mediator.Send(command, ct);
        return ApiOk(new ApiResponseWithData<CartResult>
        {
            Success = true,
            Message = "Cart updated successfully",
            Data = result
        });
    }

    /// <summary>Deletes a cart by ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCart([FromRoute] Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCartCommand(id), ct);
        return ApiOk(new ApiResponse { Success = true, Message = "Cart deleted successfully" });
    }
}
