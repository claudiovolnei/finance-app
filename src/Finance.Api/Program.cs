using System.Net;
using System.Text;
using Finance.Api.Extensions;
using Finance.Infrastructure.Persistence;
using Finance.Infrastructure.Repositories;
using Finance.Application.Repositories;
using Finance.Application.UseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT Bearer support in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure database provider (SQL Server)
var financeConn = builder.Configuration.GetConnectionString("FinanceDb");
if (string.IsNullOrWhiteSpace(financeConn))
    throw new InvalidOperationException("ConnectionStrings:FinanceDb not configured");

builder.Services.AddDbContext<AppDbContext>(opt =>
       opt.UseSqlServer(financeConn));

// Repositories
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

// Use Cases
builder.Services.AddScoped<CreateTransactionUseCase>();
builder.Services.AddScoped<UpdateTransactionUseCase>();
builder.Services.AddScoped<DeleteTransactionUseCase>();
builder.Services.AddScoped<CreateCategoryUseCase>();
builder.Services.AddScoped<UpdateCategoryUseCase>();
builder.Services.AddScoped<DeleteCategoryUseCase>();

// JWT and auth
builder.Services.AddSingleton<Finance.Api.Services.JwtService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key not configured");
        var issuer = builder.Configuration["Jwt:Issuer"] ?? "FinanceApi";
        var audience = builder.Configuration["Jwt:Audience"] ?? "FinanceMobile";
        var signingKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey
        };
    });

// Add authorization services
builder.Services.AddAuthorization();

// Simple health checks registration
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

// Database migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapEndpoints();

// Health endpoint - simple JSON summary
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});

if (!app.Environment.IsDevelopment())
{
    var swaggerUser = builder.Configuration["SwaggerAuth:Username"];
    var swaggerPassword = builder.Configuration["SwaggerAuth:Password"];

    if (!string.IsNullOrWhiteSpace(swaggerUser) && !string.IsNullOrWhiteSpace(swaggerPassword))
    {
        app.UseWhen(context => context.Request.Path.StartsWithSegments("/swagger"), branch =>
        {
            branch.Use(async (context, next) =>
            {
                var authHeader = context.Request.Headers.Authorization.ToString();
                if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var encodedCredentials = authHeader["Basic ".Length..].Trim();
                        var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                        var parts = decodedCredentials.Split(':', 2);

                        if (parts.Length == 2 && parts[0] == swaggerUser && parts[1] == swaggerPassword)
                        {
                            await next();
                            return;
                        }
                    }
                    catch (FormatException) { }
                }

                context.Response.Headers.WWWAuthenticate = "Basic realm=\"Swagger\"";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            });
        });
    }
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.InjectJavascript("/swagger/swagger-auth.js");
});

app.Run();
