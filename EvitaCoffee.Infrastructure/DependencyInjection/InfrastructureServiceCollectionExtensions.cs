using EvitaCoffee.Application.Abstractions;
using EvitaCoffee.Application.Services.Auth;
using EvitaCoffee.Infrastructure.Identity;
using EvitaCoffee.Infrastructure.Persistence;
using EvitaCoffee.Infrastructure.Redis;
using EvitaCoffee.Infrastructure.Services.Auth;
using EvitaCoffee.Infrastructure.Services.Redis;
using EvitaCoffee.Infrastructure.Sms;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EvitaCoffee.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        // DbContext
        services.AddDbContext<AppDbContext>(options => 
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure()
            ));
        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ => 
          ConnectionMultiplexer.Connect(
            config.GetConnectionString("RedisConnection")!
          ));
        // Identity
        services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.User.RequireUniqueEmail = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Services 
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ISmsSender, DevSmsSender>();
        services.AddScoped<IRefreshTokenStore, RedisRefreshTokenStore>();
        services.AddScoped<IOtpStore, RedisOptStore>();

        return services;
        
    }
}
