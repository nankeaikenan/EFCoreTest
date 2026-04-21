using EFCoreDemo.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreDemo.Application
{
    /// <summary>
    /// Application 层的依赖注入扩展方法。
    /// 在此集中注册所有 Application 层的服务，保持 Program.cs 干净整洁。
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // ── 业务服务注册（Scoped：每次 HTTP 请求创建一个新实例）──────────
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IOrderService, OrderService>();

            // ── 认证服务注册 ───────────────────────────────────────────────────
            // AuthService 依赖 UserManager（Scoped），因此它本身也必须是 Scoped
            services.AddScoped<IAuthService, AuthService>();

            // ── AutoMapper 注册 ────────────────────────────────────────────────
            // 扫描当前程序集（Application 层）中的所有 Profile 类并自动注册
            services.AddAutoMapper(System.Reflection.Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
