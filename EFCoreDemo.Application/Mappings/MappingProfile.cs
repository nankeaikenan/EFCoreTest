using AutoMapper;
using EFCoreDemo.Domain.Models;
using EFCoreDemo.Application.DTOs.Product;
using EFCoreDemo.Application.DTOs.Category;
using EFCoreDemo.Application.DTOs.Order;

namespace EFCoreDemo.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category mappings
            CreateMap<Category, CategoryResponse>();

            // Product mappings
            CreateMap<Product, ProductResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            // Order mappings
            CreateMap<Order, OrderResponse>();
            
            CreateMap<OrderDetail, OrderDetailResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));
        }
    }
}
