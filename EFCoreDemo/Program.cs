using EFCoreDemo.Application;
using EFCoreDemo.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(); // 注册控制器服务
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册基础设施层服务 (该方法包含由 EF Core 提供的 DbContext 及其相关仓储的注册)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 注册应用层服务
builder.Services.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();


app.MapControllers(); // 映射通过 Controller 定义的所有接口

app.Run();

