using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Serilog;
using TaskManager.Api.Middlewares;

namespace TaskManager.Api;

public static class DependencyInjection
{
    public const string FrontendCorsPolicy = "Frontend";

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problem = context.HttpContext.RequestServices
                        .GetRequiredService<ProblemDetailsFactory>()
                        .CreateValidationProblemDetails(
                            context.HttpContext,
                            context.ModelState,
                            StatusCodes.Status400BadRequest,
                            "El cuerpo o los parámetros de la petición no tienen un formato válido.");

                    return new BadRequestObjectResult(problem);
                });
        services.AddOpenApi();

        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context =>
                context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.TraceIdentifier));
        services.AddExceptionHandler<GlobalExceptionHandler>();

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options =>
            options.AddPolicy(FrontendCorsPolicy, policy =>
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

        AddLogsConnectionString(configuration);

        services.AddSerilog((provider, logger) => logger
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(provider));

        return services;
    }

    public const string LogsConnectionStringName = "TaskManagerLogs";

    private static void AddLogsConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(TaskManager.Infrastructure.DependencyInjection.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var builder = new SqlConnectionStringBuilder(connectionString) { MinPoolSize = 0, MaxPoolSize = 1 };
        configuration[$"ConnectionStrings:{LogsConnectionStringName}"] = builder.ConnectionString;
    }
}
