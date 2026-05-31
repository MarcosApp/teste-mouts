using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

[ApiController]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>Creates a new sale.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleRequestValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var command = _mapper.Map<CreateSaleCommand>(request);
        var result = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Sale created successfully",
            Data = _mapper.Map<CreateSaleResponse>(result)
        });
    }

    /// <summary>Retrieves a paginated list of sales.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseWithData<PaginatedResponse<GetSaleResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSales(
        [FromQuery] int _page = 1,
        [FromQuery] int _size = 10,
        [FromQuery] string? _order = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? customerName = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] string? saleNumber = null,
        [FromQuery] bool? isCancelled = null,
        [FromQuery] DateTime? _minDate = null,
        [FromQuery] DateTime? _maxDate = null,
        [FromQuery] decimal? _minTotalAmount = null,
        [FromQuery] decimal? _maxTotalAmount = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ListSalesQuery
        {
            Page = _page, Size = _size, Order = _order,
            CustomerId = customerId, CustomerName = customerName,
            BranchId = branchId, SaleNumber = saleNumber,
            IsCancelled = isCancelled,
            MinDate = _minDate, MaxDate = _maxDate,
            MinTotalAmount = _minTotalAmount, MaxTotalAmount = _maxTotalAmount
        };
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponseWithData<object>
        {
            Success = true,
            Message = "Sales retrieved successfully",
            Data = new
            {
                data = _mapper.Map<IEnumerable<GetSaleResponse>>(result.Sales),
                totalItems = result.TotalCount,
                currentPage = result.CurrentPage,
                totalPages = result.TotalPages
            }
        });
    }

    /// <summary>Retrieves a sale by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSaleCommand(id), cancellationToken);
        return Ok(new ApiResponseWithData<GetSaleResponse>
        {
            Success = true,
            Message = "Sale retrieved successfully",
            Data = _mapper.Map<GetSaleResponse>(result)
        });
    }

    /// <summary>Updates an existing sale.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSale([FromRoute] Guid id, [FromBody] UpdateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleRequestValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var command = _mapper.Map<UpdateSaleCommand>(request);
        command.Id = id;

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(new ApiResponseWithData<UpdateSaleResponse>
        {
            Success = true,
            Message = "Sale updated successfully",
            Data = _mapper.Map<UpdateSaleResponse>(result)
        });
    }

    /// <summary>Deletes a sale by ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSaleCommand(id), cancellationToken);
        return Ok(new ApiResponse { Success = true, Message = "Sale deleted successfully" });
    }

    /// <summary>Cancels a sale by ID.</summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<CancelSaleResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelSaleCommand(id), cancellationToken);
        return Ok(new ApiResponseWithData<CancelSaleResult>
        {
            Success = true,
            Message = "Sale cancelled successfully",
            Data = result
        });
    }

    /// <summary>Cancels a specific item within a sale.</summary>
    [HttpPatch("{id:guid}/items/{itemId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<CancelSaleItemResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSaleItem([FromRoute] Guid id, [FromRoute] Guid itemId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelSaleItemCommand(id, itemId), cancellationToken);
        return Ok(new ApiResponseWithData<CancelSaleItemResult>
        {
            Success = true,
            Message = "Item cancelled successfully",
            Data = result
        });
    }
}
