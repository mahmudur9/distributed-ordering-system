using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using ObjectStoreService.API.Middlewares;
using ObjectStoreService.Application.Extensions;
using ObjectStoreService.Infrastructure.Extensions;
using Prometheus;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// builder.Services.AddGrpc();

// Configure Kestrel for HTTP/2 without TLS
builder.WebHost.ConfigureKestrel(options =>
{
    /*options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });*/

    // Web API on port 8080 (HTTP/1.1)
    options.ListenAnyIP(8000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

// Register extensions
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

// RateLimiter
builder.Services.AddRateLimiter(options =>
{
    options.AddConcurrencyLimiter("concurrent", opt =>
    {
        opt.PermitLimit = 15;
        opt.QueueLimit = 30;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// Configure serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", "ObjectStoreService")
    .Enrich.WithProperty("environment", "production")
    .Filter.ByExcluding("RequestPath = '/metrics'") // Exclude prometheus logs
    .WriteTo.Console(new RenderedCompactJsonFormatter()) // This line is responsible for producing the docker logs
    .WriteTo.File( // This line writes logs in files inside the container
        new RenderedCompactJsonFormatter(),
        "logs/objectstoreservice-.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");

app.UseStaticFiles();

// app.UseAuthentication();

app.UseAuthorization();

// Prometheus metrics
app.MapMetrics();

// app.MapGrpcService<GreeterService>();
app.MapControllers().RequireRateLimiting("concurrent");

app.Run();