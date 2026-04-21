using EFCoreDemo.Application.DTOs.Auth;

namespace EFCoreDemo.Application.Services
{
    /// <summary>
    /// 认证服务接口，定义用户身份验证的核心契约。
    ///
    /// 【接口设计原则 - 依赖倒置原则（DIP）】
    ///   控制器（AuthController）依赖此接口而非具体实现（AuthService），
    ///   这使得：
    ///     1. 控制器与认证逻辑解耦，便于单元测试（可注入 Mock 实现）
    ///     2. 未来可无缝切换不同认证策略（如 OAuth2、外部 SSO）
    ///     3. 符合 Clean Architecture 中"Application 层定义契约"的规范
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 注册新用户。
        /// </summary>
        /// <param name="request">包含邮箱、密码、姓名和角色的注册信息</param>
        /// <returns>
        ///   成功时：返回含 Token 的 AuthResponse
        ///   失败时：返回 null（调用方根据 null 判断失败并返回 400/409）
        /// </returns>
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// 用户登录，验证凭据并颁发 JWT + Refresh Token。
        /// </summary>
        /// <param name="request">邮箱和密码</param>
        /// <returns>成功返回 AuthResponse，凭据错误返回 null</returns>
        Task<AuthResponse?> LoginAsync(LoginRequest request);

        /// <summary>
        /// 使用 Refresh Token 换取新的 Access Token（静默刷新）。
        /// </summary>
        /// <param name="request">旧的过期 AccessToken + RefreshToken</param>
        /// <returns>新的 AuthResponse（含新双 Token），失败返回 null</returns>
        Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);

        /// <summary>
        /// 撤销 Refresh Token（实现登出）。
        /// 将数据库中该用户的 RefreshToken 字段置空，
        /// 使后续刷新请求无效化（即使攻击者持有旧 Refresh Token 也无法使用）。
        /// </summary>
        /// <param name="email">要登出的用户邮箱</param>
        /// <returns>操作是否成功</returns>
        Task<bool> RevokeTokenAsync(string email);
    }
}
