using EFCoreDemo.Domain.Interfaces;
using EFCoreDemo.Infrastructure.Data;
using EFCoreDemo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EFCoreDemo.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 0)),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            return services;
        }
    }
}
