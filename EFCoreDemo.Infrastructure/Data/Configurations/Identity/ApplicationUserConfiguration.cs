using EFCoreDemo.Domain.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCoreDemo.Infrastructure.Data.Configurations.Identity
{
    /// <summary>
    /// ApplicationUser 的 EF Core Fluent API 配置类。
    ///
    /// 使用 Fluent API 的好处：
    ///   1. 不污染领域模型（无需在 ApplicationUser 上写 DataAnnotations）
    ///   2. 配置更灵活，可处理复杂关系、索引、表名映射等
    ///   3. 与 Clean Architecture 原则一致（领域层不依赖 EF Core）
    ///
    /// 注意：此类通过 ApplyConfigurationsFromAssembly 自动被 DbContext 加载，
    ///       无需在 DbContext.OnModelCreating 中手动注册。
    /// </summary>
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // ────────────────────────────────────────────────
            // 字段约束配置
            // ────────────────────────────────────────────────

            // FullName：用户真实姓名，最大 100 字符，不允许 null（模型层已默认 string.Empty）
            builder.Property(u => u.FullName)
                .HasMaxLength(100)
                .IsRequired();

            // CreatedAt：注册时间，数据库列设为不可空，默认值由应用层（Service）赋值
            builder.Property(u => u.CreatedAt)
                .IsRequired();

            // RefreshToken：可为 null（未登录或已登出用户）
            // 最大长度 500 是安全随机字符串的合理上限
            builder.Property(u => u.RefreshToken)
                .HasMaxLength(500);

            // RefreshTokenExpiryTime：可为 null
            builder.Property(u => u.RefreshTokenExpiryTime)
                .IsRequired(false);

            // ────────────────────────────────────────────────
            // 索引配置（Identity 已自动在 Email/UserName 上建立唯一索引）
            // 此处我们无需重复；如有额外查询需求，可在下方添加
            // ────────────────────────────────────────────────
        }
    }
}
