using EvitaCoffee.API.ApiDependencyInjection;
using EvitaCoffee.API.Middleware;
using EvitaCoffee.Infrastructure.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Evita coffee web host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .WriteTo.File("Logs/EvitaCoffee.txt", rollingInterval: RollingInterval.Day)
    );

    // Modular DI
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddHealthCheckServices(builder.Configuration);
    // Infrastructure
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddAuthenticationCore(builder.Configuration);


    


    // Add services to the container

    builder.Services.AddOpenApi();
    

    var app = builder.Build();

   

    // Configure the HTTP request pipeline.

    // A middleware for logging every request 
    app.UseSerilogRequestLogging(configure =>
    {
        configure.MessageTemplate = "HTTP {RequestMethod} {RequestPath} {UserId} responded {StatusCode} in {Elapsed:0.0000} ms";

    });


    if (app.Environment.IsDevelopment())
    {
        //app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Evita Coffee API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseExceptionHandler();
    app.UseMiddleware<CorrelationMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseCors("AllowedFrontendOrigins");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health/live");
    app.MapHealthChecks("/health/ready");
    app.MapControllers();

    // TODO: app.MigrateDatabaseAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
return 0;



