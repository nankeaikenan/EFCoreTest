using EFCoreDemo.Application;
using EFCoreDemo.Infrastructure;
using EFCoreDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ════════════════════════════════════════════════════════════════════════════
//  一、注册服务（Service Registration）
//  规则：所有 builder.Services.AddXxx() 必须在 builder.Build() 之前完成
// ════════════════════════════════════════════════════════════════════════════

// ── 1. 控制器服务 ──────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── 2. Infrastructure 层（DbContext + Identity + Repositories）────────────
// 注意：AddIdentity 在 Infrastructure 层内部注册，顺序必须在 AddAuthentication 之前
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── 3. Application 层（业务服务 + AuthService + AutoMapper）──────────────
builder.Services.AddApplicationServices();

// ── 4. JWT Bearer 认证配置 ─────────────────────────────────────────────────
//
// 【认证（Authentication）vs 授权（Authorization）区别】
//   Authentication（认证）：确认"你是谁"  → JWT 解析、验签
//   Authorization（授权）：确认"你能做什么" → [Authorize] / 角色/策略检查
//
// 此处配置 JWT Bearer 作为默认的认证方案（Scheme）
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey 配置缺失，请检查 appsettings.json");

builder.Services.AddAuthentication(options =>
{
    // 声明默认认证方案，框架在处理 [Authorize] 时使用此方案
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;    // 未认证时的失败响应方案
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // SaveToken：是否将验证通过的 Token 保存到 HttpContext.Request.Headers（通常不需要）
    options.SaveToken = true;

    // RequireHttpsMetadata：生产环境应设为 true，强制 HTTPS 传输 Token
    options.RequireHttpsMetadata = false; // 开发环境关闭，生产改为 true

    // TokenValidationParameters：JWT 验证规则（核心配置）
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // ── 颁发者验证 ────────────────────────────────────────────────────
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],            // 必须与 Token 中 iss 一致

        // ── 受众验证 ──────────────────────────────────────────────────────
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],         // 必须与 Token 中 aud 一致

        // ── 签名验证（最关键！防伪造）────────────────────────────────────
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)),

        // ── 有效期验证 ────────────────────────────────────────────────────
        ValidateLifetime = true,                         // 验证 exp 声明（过期时间）

        // 时钟偏差容忍度：允许服务器时钟相差 0 秒（严格模式）
        // 默认是 5 分钟，可能导致 Token 实际多活 5 分钟，建议设为 0
        ClockSkew = TimeSpan.Zero,

        // ── Claims 映射 ───────────────────────────────────────────────────
        // 关闭 Microsoft 的 Claims 名称自动映射
        // 不设置时，ClaimTypes.NameIdentifier 会被映射为长 URL 格式的字符串
        // 设为 false 后，JWT 声明名称保持原样（如 "sub", "email"）
        // 本项目使用 ClaimTypes.* 标准名称，所以保持默认 true 即可
        // NameClaimType = ClaimTypes.Name,
        // RoleClaimType = ClaimTypes.Role,
    };

    // ── JWT 事件回调（可选，用于调试）────────────────────────────────────
    options.Events = new JwtBearerEvents
    {
        // Token 验证失败时触发（可在此记录日志）
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                // 向客户端响应头添加过期标志，便于前端判断是否需要刷新
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// ── 5. 授权策略（Authorization Policies）────────────────────────────────────
// AddAuthorization 支持定义命名策略，比单纯的 [Authorize(Roles="...")] 更灵活
builder.Services.AddAuthorization(options =>
{
    // 策略示例：同时具备 Admin 角色 + Email 已确认 的用户
    // 控制器中使用：[Authorize(Policy = "AdminOnly")]
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // 策略：已认证用户（任何角色均可）
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

// ── 6. Swagger / OpenAPI 配置（支持 Bearer Token 授权）──────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EFCoreDemo API",
        Version = "v1",
        Description = "ASP.NET Core Identity + JWT 企业级认证示例 API",
        Contact = new OpenApiContact
        {
            Name = "EFCoreDemo Team",
            Email = "admin@efcoredemo.com"
        }
    });

    // ── 在 Swagger UI 中添加 "Authorize" 按钮（Bearer Token 输入框）─────
    // 没有这段配置，Swagger 无法在请求头中携带 JWT，所有需要认证的接口都会 401
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "请输入 JWT Token（不需要加 'Bearer ' 前缀，Swagger 会自动添加）\n\n" +
                      "示例：eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    // 全局安全要求：所有接口默认需要 Bearer Token
    // 标注了 [AllowAnonymous] 的接口仍然可以匿名访问
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()   // 空数组表示不限制 Scopes（OAuth2 的概念，JWT 不适用）
        }
    });
});

// ════════════════════════════════════════════════════════════════════════════
//  二、构建应用（Build）
// ════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

// ════════════════════════════════════════════════════════════════════════════
//  三、数据库种子数据初始化
//  必须在 app.Run() 之前，确保角色和管理员账号存在
// ════════════════════════════════════════════════════════════════════════════
await DataSeeder.SeedAsync(app.Services);

// ════════════════════════════════════════════════════════════════════════════
//  四、配置 HTTP 请求管道（Middleware Pipeline）
//  【重要】中间件顺序固定，错误的顺序会导致认证/授权失效！
// ════════════════════════════════════════════════════════════════════════════

// ── 开发环境：启用 Swagger UI ──────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EFCoreDemo API v1");
        c.RoutePrefix = string.Empty; // 将 Swagger UI 设为根路径（http://localhost:xxxx/）
    });
}

// ── HTTPS 重定向（开发中注释掉，生产必须开启）────────────────────────────
// app.UseHttpsRedirection();

// ── 认证中间件（Authentication）─────────────────────────────────────────────
// 解析请求头中的 JWT Token，填充 HttpContext.User（ClaimsPrincipal）
// ⚠️ 必须在 UseAuthorization() 之前！
app.UseAuthentication();

// ── 授权中间件（Authorization）──────────────────────────────────────────────
// 检查 HttpContext.User 是否满足接口上的 [Authorize] 要求
// ⚠️ 必须在 UseAuthentication() 之后、MapControllers() 之前！
app.UseAuthorization();

// ── 控制器路由映射 ────────────────────────────────────────────────────────
app.MapControllers();

app.Run();
