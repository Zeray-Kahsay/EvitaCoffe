using System.Text.Json;
using System.Text.Json.Serialization;
using EvitaCoffee.API.Middleware;
using EvitaCoffee.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;


namespace EvitaCoffee.API.ApiDependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config )
    {   
        // SERVICES 
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();


        // RATE LIMITING
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("general", config =>
            {
                config.PermitLimit = 100;
                config.Window = TimeSpan.FromMinutes(1);
            });

            options.AddFixedWindowLimiter("auth", config =>
            {
                config.PermitLimit = 10;
                config.Window = TimeSpan.FromMinutes(1);
            });
        });

        services.AddControllers()
            .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
            });
        services.AddEndpointsApiExplorer();

        // DBCONTEXT 
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        });

        // SWAGGER 
        services.AddSwaggerGen(Options =>
        {
            Options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EvitaCoffee.Api",
                Version = "v1"
            });
            // 1. Define the security scheme
            Options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });
        });

        // CORS
        services.AddCors(opt =>
        {
            opt.AddPolicy("AllowedFrontendOrigins", policy =>
            {
                var allowedOrigins = config.GetSection("AllowedFrontendOrigins").Get<string[]>() ?? [];
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // IMAGE SIZE
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 10 * 1024 *1024;
        });



        return services;
    }
}
