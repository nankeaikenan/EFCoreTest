using Microsoft.AspNetCore.Identity;

namespace EFCoreDemo.Domain.Models.Identity
{
    /// <summary>
    /// 扩展 Identity 的用户实体。
    ///
    /// IdentityUser 已经内置了以下常用字段，我们无需重复定义：
    ///   - Id          (string, 主键, 默认 GUID 字符串)
    ///   - UserName    (登录用户名)
    ///   - Email       (电子邮件)
    ///   - PasswordHash(已哈希的密码，Identity 框架自动处理)
    ///   - PhoneNumber (手机号)
    ///   - LockoutEnd  (账号锁定到期时间)
    ///   - AccessFailedCount (登录失败次数)
    ///   等 ...
    ///
    /// 我们只需在此处添加"超出 Identity 标准范围"的业务字段。
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // ──────────────────────────────────────────────
        // 业务扩展字段
        // ──────────────────────────────────────────────

        /// <summary>
        /// 用户真实姓名（显示名称），最大长度 100。
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// 账号创建时间（UTC）。
        /// 在注册时由 AuthService 赋值，不依赖数据库默认值，保持跨数据库兼容。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ──────────────────────────────────────────────
        // Refresh Token 双令牌相关字段
        // ──────────────────────────────────────────────

        /// <summary>
        /// 存储在数据库中的 Refresh Token（刷新令牌）。
        ///
        /// 安全说明：
        ///   - Access Token（JWT）有效期短（如 60 分钟），存储在客户端内存或 Header 中。
        ///   - Refresh Token 有效期长（如 7 天），存储在数据库，客户端应安全存储（HttpOnly Cookie）。
        ///   - 登出时将此字段置 null，即可使旧 Refresh Token 立刻失效（类似服务端会话吊销）。
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Refresh Token 的过期时间（UTC）。
        /// 刷新时先校验此时间，防止过期 Token 被反复使用。
        /// </summary>
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
