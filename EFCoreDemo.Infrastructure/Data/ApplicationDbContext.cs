using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EFCoreDemo.Domain.Models;
using EFCoreDemo.Domain.Models.Identity;
using System.Reflection;

namespace EFCoreDemo.Infrastructure.Data
{
    /// <summary>
    /// 应用程序数据库上下文。
    ///
    /// 【重要变更】从普通的 DbContext 升级为 IdentityDbContext&lt;ApplicationUser&gt;。
    ///
    /// IdentityDbContext 会自动管理以下 9 张 Identity 标准表（迁移时自动创建）：
    ///   - AspNetUsers          —— 用户表（对应 ApplicationUser）
    ///   - AspNetRoles          —— 角色表（如 "Admin", "Customer"）
    ///   - AspNetUserRoles      —— 用户-角色关联表（多对多）
    ///   - AspNetUserClaims     —— 用户声明表（存自定义 Claim）
    ///   - AspNetUserLogins     —— 第三方登录表（如 Google、GitHub OAuth）
    ///   - AspNetUserTokens     —— 用户 Token 表（如邮箱验证 Token）
    ///   - AspNetRoleClaims     —— 角色声明表（角色级别的 Claim）
    ///
    /// 泛型参数说明：
    ///   IdentityDbContext&lt;TUser&gt; —— TUser 是我们自定义的用户类型（ApplicationUser）
    ///   如需更多自定义，可使用 IdentityDbContext&lt;TUser, TRole, TKey&gt; 的重载形式
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ──────────────────────────────────────────────────────────────
        // 原有业务实体的 DbSet（保持不变）
        // ──────────────────────────────────────────────────────────────

        /// <summary>商品表</summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>分类表</summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>订单表</summary>
        public DbSet<Order> Orders { get; set; }

        /// <summary>订单详情表</summary>
        public DbSet<OrderDetail> OrderDetails { get; set; }

        /// <summary>作者表</summary>
        public DbSet<Author> Authors { get; set; }

        /// <summary>书籍表</summary>
        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 必须先调用 base.OnModelCreating，让 Identity 完成其内部的表结构配置
            // 若不调用，Identity 相关的外键、索引将无法正确创建
            base.OnModelCreating(modelBuilder);

            // 扫描并应用当前程序集中所有实现了 IEntityTypeConfiguration<T> 的配置类
            // 包括我们自定义的 ApplicationUserConfiguration 以及原有的业务实体配置
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}