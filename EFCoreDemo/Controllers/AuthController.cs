using EFCoreDemo.Application.DTOs.Auth;
using EFCoreDemo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EFCoreDemo.Controllers
{
    /// <summary>
    /// 认证控制器：处理用户注册、登录、Token 刷新、登出等身份认证相关接口。
    ///
    /// 路由设计：api/auth/...
    ///
    /// 【[ApiController] 的作用】
    ///   1. 自动进行模型验证（DataAnnotations），验证失败自动返回 400 BadRequest
    ///   2. 自动从请求体绑定参数（不需要手写 [FromBody]）
    ///   3. 统一错误响应格式（ProblemDetails）
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// 通过构造函数注入认证服务（依赖倒置）。
        /// </summary>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // POST api/auth/register
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 注册新用户账号。
        ///
        /// 成功：返回 201 Created + AuthResponse（含 Token，注册即登录体验）
        /// 失败：返回 409 Conflict（邮箱已注册）或 400 BadRequest（密码不符合规则）
        /// </summary>
        /// <remarks>
        /// 请求示例：
        ///
        ///     POST /api/auth/register
        ///     {
        ///         "email": "user@example.com",
        ///         "password": "User@123456",
        ///         "confirmPassword": "User@123456",
        ///         "fullName": "张三",
        ///         "role": "Customer"
        ///     }
        /// </remarks>
        [HttpPost("register")]
        [AllowAnonymous]  // 明确允许匿名访问（即使全局设置了需要认证也能访问）
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (result.EmailAlreadyExists)
            {
                return Conflict(new { message = "该邮箱已被注册，请直接登录或使用其他邮箱。" });
            }

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "注册失败，请检查输入信息。", errors = result.ValidationErrors });
            }

            return StatusCode(StatusCodes.Status201Created, result.Response);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // POST api/auth/login
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 用户登录，验证邮箱和密码，成功后返回 JWT Access Token + Refresh Token。
        ///
        /// 成功：返回 200 OK + AuthResponse
        /// 失败：返回 401 Unauthorized（凭据错误或账号被锁定）
        /// </summary>
        /// <remarks>
        /// 请求示例：
        ///
        ///     POST /api/auth/login
        ///     {
        ///         "email": "admin@efcoredemo.com",
        ///         "password": "Admin@123456"
        ///     }
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                // 【安全最佳实践】不区分"用户不存在"和"密码错误"
                // 统一返回"凭据无效"，防止账号枚举攻击
                return Unauthorized(new { message = "邮箱或密码不正确，请检查后重试。" });
            }

            return Ok(response);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // POST api/auth/refresh
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 刷新 Access Token（静默续签）。
        ///
        /// 当前端检测到 Access Token 过期时（通过响应头 Token-Expired: true 或 401 响应），
        /// 调用此接口发送旧 Access Token + Refresh Token，换取新的 Token 对。
        ///
        /// 成功：返回 200 OK + 新的 AuthResponse
        /// 失败：返回 401（Refresh Token 无效/过期，需重新登录）
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request);

            if (response == null)
            {
                return Unauthorized(new { message = "Token 无效或已过期，请重新登录。" });
            }

            return Ok(response);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // POST api/auth/revoke
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 登出（吊销 Refresh Token）。
        ///
        /// [Authorize]：此接口需要有效的 Access Token（已登录状态才能登出）。
        /// 撤销成功后，该用户的 Refresh Token 立即失效，
        /// 现有 Access Token 仍有效直到自然过期（JWT 无状态特性）。
        ///
        /// 成功：返回 204 No Content
        /// 失败：返回 400 Bad Request
        /// </summary>
        [HttpPost("revoke")]
        [Authorize]  // 必须携带有效 JWT 才能调用（登出前必须已登录）
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RevokeToken()
        {
            // 从当前请求的 JWT Claims 中取出邮箱（无需客户端传参，安全且防篡改）
            // ClaimTypes.Email 在 AuthService.GenerateAccessToken 中写入了 Token
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "无法从 Token 中获取用户信息。" });
            }

            var result = await _authService.RevokeTokenAsync(email);

            if (!result)
            {
                return BadRequest(new { message = "登出失败，请重试。" });
            }

            // 204 No Content：操作成功但没有响应体（登出成功无需返回数据）
            return NoContent();
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // GET api/auth/me
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 获取当前登录用户的基本信息（从 JWT Claims 中读取，无需查数据库）。
        ///
        /// 此接口演示如何在控制器中读取 JWT Claims，
        /// 实际项目中可依此模式在任何接口中获取当前用户身份。
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            // User 是 ControllerBase 的属性，类型为 ClaimsPrincipal
            // 它由 JWT Bearer 中间件自动填充（UseAuthentication 负责解析 Token）
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var fullName = User.FindFirstValue(ClaimTypes.Name);

            // GetValues：获取所有 Role Claims（用户可能有多个角色）
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new
            {
                UserId = userId,
                Email = email,
                FullName = fullName,
                Roles = roles,
                Message = "Token 有效，当前用户信息从 JWT Claims 读取（未查数据库）。"
            });
        }
    }
}
