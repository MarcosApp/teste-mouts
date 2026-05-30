using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleProfile : Profile
{
    public CreateSaleProfile()
    {
        CreateMap<CreateSaleCommand, Sale>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<CreateSaleItemCommand, SaleItem>()
            .ConstructUsing(src => new SaleItem(src.ProductId, src.ProductName, src.Quantity, src.UnitPrice));

        CreateMap<Sale, CreateSaleResult>();
        CreateMap<SaleItem, SaleItemResult>();
    }
}
