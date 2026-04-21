using EFCoreDemo.Domain.Interfaces;
using EFCoreDemo.Domain.Models.Identity;
using EFCoreDemo.Infrastructure.Data;
using EFCoreDemo.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreDemo.Infrastructure
{
    /// <summary>
    /// Infrastructure 层的依赖注入扩展方法。
    /// 负责注册：EF Core DbContext、Identity、仓储（Repositories）等基础设施服务。
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── 1. 注册 DbContext（MySQL）──────────────────────────────────────
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 0)),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // ── 2. 注册 ASP.NET Core Identity ─────────────────────────────────
            //
            // AddIdentity<TUser, TRole>  注册核心 Identity 服务，包括：
            //   - UserManager<ApplicationUser>   用户 CRUD、密码哈希、角色管理
            //   - RoleManager<IdentityRole>      角色 CRUD
            //   - SignInManager<ApplicationUser> 登录/登出、Cookie 管理（本项目不用 Cookie，但仍需注册）
            //   - IPasswordHasher<>              密码哈希器（默认 BCrypt-like PBKDF2 算法）
            //   - IPasswordValidator<>           密码规则验证器
            //   - IUserValidator<>               用户名/邮箱规则验证器
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // ── 密码策略（Password Policy）──────────────────────────────
                // 企业标准：强密码要求
                options.Password.RequiredLength = 8;          // 最少 8 位
                options.Password.RequireUppercase = true;      // 必须含大写字母
                options.Password.RequireLowercase = true;      // 必须含小写字母
                options.Password.RequireDigit = true;          // 必须含数字
                options.Password.RequireNonAlphanumeric = true;// 必须含特殊字符（@#$%^&*）
                options.Password.RequiredUniqueChars = 4;      // 至少 4 种不同字符

                // ── 用户策略（User Policy）──────────────────────────────────
                options.User.RequireUniqueEmail = true;       // 邮箱必须唯一（数据库 Email 列有唯一索引）
                options.User.AllowedUserNameCharacters =      // 用户名允许的字符集
                    "abcdefghijklmnopqrstuvwxyz" +
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                    "0123456789@.-_+";                        // 允许 @ 使邮箱格式的用户名合法

                // ── 登录策略（SignIn Policy）────────────────────────────────
                options.SignIn.RequireConfirmedEmail = false; // 关闭邮箱验证（开发环境）
                                                              // 生产环境应改为 true

                // ── 账号锁定策略（Lockout Policy）──────────────────────────
                // 防止暴力破解（Brute Force Attack）的重要安全配置
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 锁定 5 分钟
                options.Lockout.MaxFailedAccessAttempts = 5;  // 连续失败 5 次触发锁定
                options.Lockout.AllowedForNewUsers = true;    // 新用户也受锁定保护
            })
            // 指定使用 EF Core 作为 Identity 数据存储后端
            // 这会将 UserManager/RoleManager 的操作映射到 ApplicationDbContext
            .AddEntityFrameworkStores<ApplicationDbContext>()
            // 添加默认令牌提供器（用于生成邮箱验证、密码重置等 Token）
            // 即使本项目暂时不用，也建议注册，以便将来扩展
            .AddDefaultTokenProviders();

            // ── 3. 注册业务仓储（Repository Pattern）──────────────────────────
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
