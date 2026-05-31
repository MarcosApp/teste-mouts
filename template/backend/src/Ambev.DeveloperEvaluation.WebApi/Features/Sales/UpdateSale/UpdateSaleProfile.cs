using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleProfile : Profile
{
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<CreateSaleItemRequest, CreateSaleItemCommand>();
        CreateMap<UpdateSaleResult, UpdateSaleResponse>();
        CreateMap<SaleItemResult, SaleItemResponse>();
    }
}
