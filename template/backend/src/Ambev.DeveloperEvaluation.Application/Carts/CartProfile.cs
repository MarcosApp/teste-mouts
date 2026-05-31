using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Carts;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<Cart, CartResult>();
        CreateMap<CartProduct, CartProductResult>();
    }
}
