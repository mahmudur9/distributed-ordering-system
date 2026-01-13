using OrderService.API.Middlewares;
using OrderService.Application.Extensions;
using OrderService.Infrastructure.Extensions;

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");

app.UseAuthorization();

// app.MapGrpcService<GreeterService>();
app.MapControllers();

app.Run();