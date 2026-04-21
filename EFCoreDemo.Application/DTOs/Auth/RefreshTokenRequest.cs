using System.ComponentModel.DataAnnotations;

namespace EFCoreDemo.Application.DTOs.Auth
{
    /// <summary>
    /// Refresh Token 刷新请求 DTO。
    ///
    /// 【为什么需要传旧的 Access Token？】
    ///   仅凭 Refresh Token 无法确认用户身份（因为 RefreshToken 只是随机字符串）。
    ///   同时传入过期的 Access Token，可以从其 Claims 中提取 UserId/Email，
    ///   再通过 UserId 查询数据库验证 Refresh Token 是否匹配，形成双重验证。
    ///
    ///   这防止了：攻击者只拿到 Refresh Token 却无法知道对应账号的场景。
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// 已过期（或即将过期）的 Access Token（JWT）。
        /// 服务端不验证其有效期，只读取其中的 Claims（如 UserId）。
        /// </summary>
        [Required(ErrorMessage = "AccessToken 不能为空")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 对应的 Refresh Token（存储在数据库中的随机字符串）。
        /// 服务端会查询数据库验证此值是否匹配且未过期。
        /// </summary>
        [Required(ErrorMessage = "RefreshToken 不能为空")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
