using System.ComponentModel.DataAnnotations;

namespace EFCoreDemo.Application.DTOs.Auth
{
    /// <summary>
    /// 用户注册请求 DTO。
    ///
    /// 【DTO 设计原则】
    ///   使用 DataAnnotations 在 DTO 层做"前置校验"（格式校验），
    ///   而深层业务校验（如邮箱是否已注册）则交给 Service 层处理。
    ///   这样可以在请求进入 Service 之前快速拦截无效数据，减少不必要的数据库访问。
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// 用户电子邮件，同时作为登录账号。
        /// [Required]：不能为空 | [EmailAddress]：必须符合邮箱格式。
        /// </summary>
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 登录密码。
        /// [MinLength]：强制密码最短长度，建议配合 IdentityOptions 中的 PasswordOptions 使用。
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [MinLength(6, ErrorMessage = "密码至少 6 位")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 确认密码，用于前端双重输入校验。
        /// [Compare]：验证两次输入密码是否一致（只在 DTO 层校验，不落库）。
        /// </summary>
        [Required(ErrorMessage = "确认密码不能为空")]
        [Compare(nameof(Password), ErrorMessage = "两次密码输入不一致")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// 用户真实姓名（显示名称）。
        /// </summary>
        [Required(ErrorMessage = "姓名不能为空")]
        [MaxLength(100, ErrorMessage = "姓名最多 100 字符")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// 用户角色。可选值：Admin / Customer。
        /// 默认为 Customer（不允许客户端自行申请 Admin，实际项目中此字段常由后台分配）。
        /// </summary>
        public string Role { get; set; } = "Customer";
    }
}
