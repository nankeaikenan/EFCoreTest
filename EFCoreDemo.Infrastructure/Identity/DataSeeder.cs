using EFCoreDemo.Domain.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EFCoreDemo.Infrastructure.Identity
{
    /// <summary>
    /// 数据库种子数据播种器（Data Seeder）。
    ///
    /// 【企业实践】在真实项目中，初始角色和超级管理员账号不应手动插入数据库，
    /// 而应通过代码在应用启动时自动检查并创建，确保：
    ///   1. 每次部署后都能幂等执行（已存在的数据不会重复创建）
    ///   2. 角色/账号与代码版本保持同步
    ///   3. 新环境（开发/测试/生产）部署时无需手动初始化
    ///
    /// 调用时机：在 Program.cs 中 app.Run() 之前调用 await DataSeeder.SeedAsync(app)
    /// </summary>
    public static class DataSeeder
    {
        // ──────────────────────────────────────────────────────────────────
        // 预定义角色常量
        // 使用常量而非魔法字符串，防止拼写错误导致权限混乱
        // ──────────────────────────────────────────────────────────────────

        /// <summary>管理员角色名</summary>
        public const string AdminRole = "Admin";

        /// <summary>普通客户角色名</summary>
        public const string CustomerRole = "Customer";

        /// <summary>
        /// 执行种子数据初始化。
        /// 此方法是幂等的：重复调用不会产生副作用（不会抛异常，不会重复插入）。
        /// </summary>
        /// <param name="serviceProvider">来自 app.Services 的服务提供者</param>
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            // 创建一个独立的 DI 作用域
            // 【注意】SeedAsync 在 app.Run() 之前调用，此时 HTTP 请求作用域不存在，
            //         需要手动创建 Scope 来获取 Scoped 服务（如 RoleManager, UserManager）
            using var scope = serviceProvider.CreateScope();

            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("DataSeeder");

            try
            {
                // 获取 RoleManager（管理角色的 Identity 服务）
                var roleManager = scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();

                // 获取 UserManager（管理用户的 Identity 服务）
                var userManager = scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();

                // ── Step 1：创建角色 ──────────────────────────────────
                await SeedRolesAsync(roleManager, logger);

                // ── Step 2：创建默认管理员账号 ────────────────────────
                await SeedAdminUserAsync(userManager, logger);

                logger.LogInformation("✅ 数据库种子数据初始化完毕。");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ 种子数据初始化失败，请检查数据库连接和权限。");
                throw; // 抛出异常，阻止启动（防止带缺失数据的应用上线）
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // 私有方法
        // ──────────────────────────────────────────────────────────────────

        private static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            // 需要初始化的所有角色列表
            var roles = new[] { AdminRole, CustomerRole };

            foreach (var roleName in roles)
            {
                // RoleExistsAsync：若角色已存在，返回 true，跳过创建（幂等保障）
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                    if (result.Succeeded)
                        logger.LogInformation("✅ 角色 [{Role}] 创建成功。", roleName);
                    else
                        logger.LogWarning("⚠️ 角色 [{Role}] 创建失败：{Errors}",
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                else
                {
                    logger.LogDebug("角色 [{Role}] 已存在，跳过。", roleName);
                }
            }
        }

        private static async Task SeedAdminUserAsync(
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            // ⚠️ 生产环境：管理员密码应从环境变量或 Azure Key Vault 中读取，
            //              不应硬编码在源码中！此处仅作开发演示。
            const string adminEmail = "admin@efcoredemo.com";
            const string adminPassword = "Admin@123456";

            // 通过邮件检查管理员是否已存在（幂等保障）
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin != null)
            {
                logger.LogDebug("管理员账号 [{Email}] 已存在，跳过。", adminEmail);
                return;
            }

            // 构建管理员用户对象
            var admin = new ApplicationUser
            {
                UserName = adminEmail,      // Identity 默认使用 UserName 登录（我们设置成 Email 格式）
                Email = adminEmail,
                FullName = "系统管理员",
                EmailConfirmed = true,      // 跳过邮箱验证（生产环境应通过邮件确认）
                CreatedAt = DateTime.UtcNow
            };

            // CreateAsync：使用 PasswordHasher 自动哈希密码后存入数据库
            // 密码规则由 IdentityOptions（在 Program.cs 中配置）决定
            var createResult = await userManager.CreateAsync(admin, adminPassword);

            if (!createResult.Succeeded)
            {
                logger.LogError("❌ 管理员账号创建失败：{Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            // 为管理员分配 Admin 角色
            var roleResult = await userManager.AddToRoleAsync(admin, AdminRole);

            if (roleResult.Succeeded)
                logger.LogInformation("✅ 管理员账号 [{Email}] 创建完毕并已分配 Admin 角色。", adminEmail);
            else
                logger.LogWarning("⚠️ 管理员角色分配失败：{Errors}",
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    }
}
