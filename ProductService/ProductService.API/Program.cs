using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using ProductService.API.Grpc;
using ProductService.API.Middlewares;
using ProductService.Application.Extensions;
using ProductService.Infrastructure.Extensions;

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
builder.Services.AddGrpc();

// Configure Kestrel for HTTP/2 without TLS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });

    // Web API on port 8080 (HTTP/1.1)
    options.ListenAnyIP(8000, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1; });
});

// Register extensions
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

// Remote jwt auth start
builder.Services.AddHttpClient("user-service", client =>
{
    client.BaseAddress = new Uri("http://user-service-api:8000/api/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("object-store-service", client =>
{
    client.BaseAddress = new Uri("http://object-store-service-api:8000/api/");
    // client.BaseAddress = new Uri("http://localhost:8005/api/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services
    .AddAuthentication("RemoteJwt")
    .AddScheme<AuthenticationSchemeOptions, RemoteJwtAuthHandler>(
        "RemoteJwt", null);

builder.Services.AddAuthorization();

builder.Services.AddMemoryCache();
// Remote jwt auth end

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProductService API",
        Version = "v1"
    });

    // 🔐 Add JWT Authentication support
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your JWT token}'"
    });

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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapGrpcService<ProductGrpc>();
app.MapControllers();

app.Run();