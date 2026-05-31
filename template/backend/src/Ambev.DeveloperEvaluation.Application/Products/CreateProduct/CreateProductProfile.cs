using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Products.CreateProduct;

public class CreateProductProfile : Profile
{
    public CreateProductProfile()
    {
        CreateMap<CreateProductCommand, Product>()
            .ForMember(d => d.Rating, o => o.MapFrom(s => new Rating(s.Rating.Rate, s.Rating.Count)));

        CreateMap<Product, ProductResult>()
            .ForMember(d => d.Rating, o => o.MapFrom(s => new RatingDto { Rate = s.Rating.Rate, Count = s.Rating.Count }));

        CreateMap<Rating, RatingDto>();
    }
}
