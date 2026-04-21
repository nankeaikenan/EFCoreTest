using System.ComponentModel.DataAnnotations;

namespace EFCoreDemo.Application.DTOs.Auth
{
    /// <summary>
    /// 用户登录请求 DTO。
    /// 只需要最基本的两个字段：邮箱（账号）和密码。
    /// </summary>
    public class LoginRequest
    {
        /// <summary>登录邮箱（账号）</summary>
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; } = string.Empty;

        /// <summary>登录密码（明文，框架自动验证哈希）</summary>
        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; } = string.Empty;
    }
}
