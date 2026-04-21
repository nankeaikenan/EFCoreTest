namespace EFCoreDemo.Application.DTOs.Auth
{
    /// <summary>
    /// 认证响应 DTO（登录/刷新 Token 成功后返回给客户端）。
    ///
    /// 【双 Token 机制说明】
    ///   Access Token（JWT）：
    ///     - 有效期短（如 60 分钟），每次请求携带在 Authorization Header 中。
    ///     - 无状态，服务端无需存储，验证只需公钥/密钥即可。
    ///     - 泄露后无法主动撤销（只能等待自然过期）。
    ///
    ///   Refresh Token：
    ///     - 有效期长（如 7 天），存储在数据库 +客户端（HttpOnly Cookie 或安全存储）。
    ///     - 有状态，服务端可随时通过清数据库实现主动吊销（登出）。
    ///     - 不直接访问业务接口，只用于换取新的 Access Token。
    ///
    ///   这种组合兼顾了"性能（无状态验证）"和"安全（可吊销）"两个需求。
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// JWT Access Token，客户端放入 Authorization: Bearer {token} Header 使用。
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh Token，用于在 Access Token 过期后换取新 Token 对。
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Access Token 过期时间（UTC），客户端可据此提前刷新。
        /// </summary>
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// 登录用户的基础信息（避免客户端再次请求用户接口）。
        /// </summary>
        public UserInfo User { get; set; } = new();
    }

    /// <summary>
    /// 用户摘要信息（嵌套在 AuthResponse 中返回）。
    /// 注意：绝不能将密码哈希等敏感字段包含在响应中。
    /// </summary>
    public class UserInfo
    {
        /// <summary>用户 ID</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>电子邮件</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>真实姓名</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>用户所属角色列表（一个用户可能有多个角色）</summary>
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
