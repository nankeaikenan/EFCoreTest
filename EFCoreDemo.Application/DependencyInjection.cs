using EFCoreDemo.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreDemo.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IOrderService, OrderService>();

            // 注册 AutoMapper，扫描当前程序集中的全部 Profile
            services.AddAutoMapper(System.Reflection.Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
