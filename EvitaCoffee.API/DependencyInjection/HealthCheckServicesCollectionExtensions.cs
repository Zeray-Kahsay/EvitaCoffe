using System;


namespace EvitaCoffee.API.ApiDependencyInjection;

public static class HealthCheckCollectionServices
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddHealthChecks()
            .AddSqlServer(
                connectionString: config.GetConnectionString("DefaultConnection") ?? ""
            )
            .AddRedis(config.GetConnectionString("RedisConnection") ?? "");

        return services;
    }

}
