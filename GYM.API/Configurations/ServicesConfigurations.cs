using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

namespace GYM.API.Configurations;

public static class ServicesConfigurations
{
    public static IServiceCollection ConfigureOpenApi(
        this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    public static IServiceCollection ConfigureSerilog(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.File(
                Path.Combine(Constants.Paths.LogsFolderPath, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger);
        });

        return services;
    }

    public static IServiceCollection ConfigureDatabase(
        this IServiceCollection services, WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<GymDbContext>(options =>
        {
            builder.Services.AddHealthChecks();
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(new LoggingInterceptor());
        });
        return services;
    }

    public static IServiceCollection ConfigureRateLimitting(
        this IServiceCollection services, WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                _ => RateLimitPartition.GetSlidingWindowLimiter(
                    "global",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    public static IServiceCollection ConfigureRedis(
        this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                builder.Configuration.GetConnectionString("Redis");

            options.InstanceName = "GYM:";
        });

        return services;
    }

    public static IServiceCollection ConfigureCors(this IServiceCollection services, WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("GymClient", policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:5246",
                        "https://localhost:7141",
                        "http://localhost:5246",
                        "https://localhost:8080",
                        "https://localhost:8081",
                        "https://gym.mohamedragheb.dev")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        return services;
    }

    public static IServiceCollection ConfigureResponseCompression(
        this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        return services;
    }

    public static IServiceCollection ConfigureAuthentication(
        this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!
                        )
                    ),

                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

        return services;
    }
}