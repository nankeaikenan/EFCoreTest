using EFCoreDemo.Application.DTOs.Auth;
using EFCoreDemo.Domain.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EFCoreDemo.Application.Services
{
    /// <summary>
    /// 认证服务的核心实现，包含：
    ///   1. 用户注册（Register）
    ///   2. 用户登录（Login）
    ///   3. Refresh Token 刷新（RefreshToken）
    ///   4. 登出吊销（RevokeToken）
    ///
    /// 依赖注入说明：
    ///   - UserManager&lt;ApplicationUser&gt;：Identity 提供的用户管理器，封装了增删查改、密码验证、角色分配等操作。
    ///   - IConfiguration：读取 appsettings.json 中的 JwtSettings 节点。
    /// </summary>
    public class AuthService : IAuthService
    {
        // ── 私有依赖 ──────────────────────────────────────────────────────────

        /// <summary>
        /// Identity 用户管理器（Scoped 生命周期）。
        /// 提供：CreateAsync / FindByEmailAsync / CheckPasswordAsync / AddToRoleAsync 等方法。
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// 应用配置，用于读取 JwtSettings（密钥、颁发者、有效期等）。
        /// </summary>
        private readonly IConfiguration _configuration;

        // ── 构造函数注入 ──────────────────────────────────────────────────────

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // ① 用户注册
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 注册新用户并颁发 Token。
        /// 流程：构建用户对象 → 创建用户（密码自动哈希）→ 分配角色 → 生成双 Token → 返回
        /// </summary>
        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            // ── Step 1：检查邮箱是否已注册 ──────────────────────────────────
            // FindByEmailAsync 执行一次数据库查询（通过 AspNetUsers 表的 Email 索引，效率高）
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                // 返回 null，由 Controller 转化为 409 Conflict 响应
                return null;
            }

            // ── Step 2：构建 ApplicationUser 对象 ───────────────────────────
            var user = new ApplicationUser
            {
                // UserName 与 Email 保持一致，是最常见的企业实践
                // （避免用户记两套账号名）
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                // EmailConfirmed = true 跳过邮件确认步骤（演示用）
                // 生产环境应为 false，并通过邮件发送确认链接
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            // ── Step 3：创建用户 ─────────────────────────────────────────────
            // CreateAsync 内部会：
            //   1. 执行 PasswordOptions 规则校验（大写、数字、特殊字符等）
            //   2. 使用 PasswordHasher 对明文密码进行 BCrypt 哈希
            //   3. 将用户记录插入 AspNetUsers 表
            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                // Identity 会返回详细错误（如密码太简单、邮箱格式错误等）
                // 此处可记录日志，Controller 返回 400 BadRequest
                return null;
            }

            // ── Step 4：分配角色 ─────────────────────────────────────────────
            // 角色校验：只允许已存在的角色（由 DataSeeder 初始化）
            // 若请求的角色不在白名单，强制使用默认角色 "Customer"
            var validRoles = new[] { "Admin", "Customer" };
            var roleToAssign = validRoles.Contains(request.Role) ? request.Role : "Customer";

            // AddToRoleAsync：向 AspNetUserRoles 表插入一条关联记录
            await _userManager.AddToRoleAsync(user, roleToAssign);

            // ── Step 5：生成 Token 并返回 ────────────────────────────────────
            return await BuildAuthResponseAsync(user);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // ② 用户登录
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 验证用户凭据并颁发令牌。
        /// 流程：查找用户 → 验证密码 → 检查账号状态 → 生成双 Token → 保存 RefreshToken
        /// </summary>
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            // ── Step 1：通过邮箱查找用户 ─────────────────────────────────────
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // 【安全设计】不区分"用户不存在"和"密码错误"的响应，
                // 防止攻击者通过不同错误消息枚举有效账号（账号枚举攻击）
                return null;
            }

            // ── Step 2：验证密码 ─────────────────────────────────────────────
            // CheckPasswordAsync：内部使用 PasswordHasher 将明文密码哈希后与库中比对
            // 同时会处理 AccessFailedCount（连续失败次数追踪，配合 Lockout 功能）
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                return null;
            }

            // ── Step 3：检查账号是否被锁定 ──────────────────────────────────
            // IsLockedOutAsync：检查 LockoutEnd 字段是否大于当前时间
            if (await _userManager.IsLockedOutAsync(user))
            {
                // 被锁定的账号不应提供任何 Token
                return null;
            }

            // ── Step 4：生成 Token 并保存 Refresh Token ──────────────────────
            return await BuildAuthResponseAsync(user);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // ③ 刷新 Access Token
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 用旧的（可能已过期的）Access Token + Refresh Token 换取新的 Token 对。
        ///
        /// 验证链：
        ///   旧 Access Token Claims 中取 UserId
        ///   → 数据库查找用户
        ///   → 对比 Refresh Token 字符串
        ///   → 检查 Refresh Token 是否过期
        ///   → 全部通过后签发新 Token 对
        /// </summary>
        public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // ── Step 1：从旧 Access Token 中提取 Claims ──────────────────────
            // 注意：此处使用 GetPrincipalFromExpiredToken，跳过有效期验证
            // （Access Token 过期是正常场景，我们只关心其中的身份信息 Claims）
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                // Token 签名无效或结构错误（不是我们签发的 Token）
                return null;
            }

            // ── Step 2：从 Claims 中取 UserId ───────────────────────────────
            // ClaimTypes.NameIdentifier 是 JWT Sub 声明，我们在生成 Token 时写入了 user.Id
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            // ── Step 3：查数据库，验证 Refresh Token ────────────────────────
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null
                || user.RefreshToken != request.RefreshToken     // 字符串完全匹配
                || user.RefreshTokenExpiryTime <= DateTime.UtcNow) // 未过期
            {
                return null;
            }

            // ── Step 4：所有验证通过，签发新 Token 对 ───────────────────────
            return await BuildAuthResponseAsync(user);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // ④ 登出（吊销 Refresh Token）
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 服务端登出：将数据库中该用户的 Refresh Token 置空。
        /// 此后该用户的 Access Token 仍有效直到自然过期（无状态Token无法立即吊销），
        /// 但 Refresh Token 已失效，即使 Access Token 过期也无法自动续签，
        /// 相当于"强制重新登录"。
        /// </summary>
        public async Task<bool> RevokeTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            // 清空 Refresh Token，使其无效化
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            // UpdateAsync：将 user 的变更（RefreshToken 字段）写回数据库
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 私有辅助方法
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 统一构建 AuthResponse：
        ///   1. 生成新的 Access Token
        ///   2. 生成新的 Refresh Token
        ///   3. 将 Refresh Token 保存到数据库
        ///   4. 组装并返回 AuthResponse
        /// </summary>
        private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user)
        {
            // 获取用户角色列表（查 AspNetUserRoles 关联表）
            var roles = await _userManager.GetRolesAsync(user);

            // 读取 JWT 配置
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "60");
            var refreshDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");

            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(expirationMinutes);

            // 生成 JWT Access Token（无状态，包含用户身份 Claims）
            var accessToken = GenerateAccessToken(user, roles, accessTokenExpiry);

            // 生成 Refresh Token（有状态，存到数据库）
            var refreshToken = GenerateRefreshToken();

            // 将 Refresh Token 持久化到数据库
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshDays);
            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiry,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Roles = roles
                }
            };
        }

        /// <summary>
        /// 生成 JWT Access Token。
        ///
        /// JWT 结构（三段式，Base64 编码，用 . 分隔）：
        ///   Header.Payload.Signature
        ///
        ///   Header：算法类型（HS256）+ 令牌类型（JWT）
        ///   Payload（Claims）：用户身份信息，任何持有者皆可解码，【不要放敏感信息】
        ///   Signature：用密钥对 Header+Payload 签名，防伪造，只有服务端能验证
        /// </summary>
        private string GenerateAccessToken(
            ApplicationUser user,
            IList<string> roles,
            DateTime expiry)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            // ── Claims（声明）：JWT Payload 中携带的用户信息 ─────────────────
            // 这些信息会被解码并注入到 HttpContext.User 中，无需查数据库即可知道用户身份
            var claims = new List<Claim>
            {
                // Sub（Subject）：JWT 标准声明，通常放用户唯一标识
                new Claim(ClaimTypes.NameIdentifier, user.Id),

                // Email 声明：控制器中可通过 User.FindFirst(ClaimTypes.Email).Value 获取
                new Claim(ClaimTypes.Email, user.Email!),

                // 姓名声明
                new Claim(ClaimTypes.Name, user.FullName),

                // JTI（JWT ID）：每个 Token 的唯一标识符，可用于 Token 黑名单场景
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                // 颁发时间（IssuedAt）
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            // 将每个角色都作为独立的 Role Claim 加入
            // 这样 [Authorize(Roles = "Admin")] 才能正确工作
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // ── 密钥 & 签名算法 ───────────────────────────────────────────────
            // 【重要】SecretKey 必须足够长（≥ 32 字符），否则 HS256 会抛异常
            //         生产环境应从 Azure Key Vault / 环境变量中读取，绝不能硬编码
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey 配置缺失");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // HmacSha256 是最常用的 JWT 签名算法，适合对称密钥（服务端自签自验）
            // 如需非对称（如公钥/私钥分离），可改用 RS256
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // ── 构建 JWT Token 描述对象 ───────────────────────────────────────
            var tokenDescriptor = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],          // 颁发者（iss 声明）
                audience: jwtSettings["Audience"],      // 受众（aud 声明）
                claims: claims,
                notBefore: DateTime.UtcNow,             // Token 生效时间（nbf 声明）
                expires: expiry,                        // 过期时间（exp 声明）
                signingCredentials: credentials
            );

            // ── 序列化为字符串 ────────────────────────────────────────────────
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        /// <summary>
        /// 生成 Refresh Token（加密随机字符串）。
        ///
        /// 使用 RandomNumberGenerator（密码学安全随机数生成器），
        /// 比 Random 类更安全，不可预测，适合安全令牌场景。
        ///
        /// 每次登录/刷新都生成全新的 Refresh Token（Rotation 旋转策略），
        /// 防止已泄露的旧 Refresh Token 被滥用（Token Rotation 安全实践）。
        /// </summary>
        private static string GenerateRefreshToken()
        {
            // 生成 64 字节随机数，Base64 编码后约 88 字符
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// 从已过期的 JWT Token 中提取 ClaimsPrincipal（不验证有效期）。
        ///
        /// 【核心技巧】TokenValidationParameters 中将 ValidateLifetime 设为 false，
        /// 这样即使 Token 已过期，只要签名有效，仍能成功解析 Claims。
        /// 这是 Refresh Token 场景的标准处理方式。
        /// </summary>
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey 配置缺失");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],

                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

                // ── 关键配置：不验证有效期 ─────────────────────────────────
                // 此处故意跳过过期验证，因为刷新场景下 Access Token 已过期是正常的
                ValidateLifetime = false
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();

                // ValidateToken：验证签名 + 结构，提取 ClaimsPrincipal
                var principal = handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                // 双重验证：确保使用的是我们预期的 HMAC-SHA256 算法
                // 防止"None 算法攻击"（攻击者发送 alg:none 的 Token）
                if (validatedToken is not JwtSecurityToken jwtToken
                    || !jwtToken.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                // Token 解析失败（被篡改、格式错误等）
                return null;
            }
        }
    }
}
