using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Abstractions.Tasks;
using TaskManager.Domain.Interfaces.Persistence;
using TaskManager.Domain.Interfaces.Tasks;
using TaskManager.Domain.Interfaces.Users;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Repositories.Dapper;
using TaskManager.Infrastructure.Repositories.EntityFramework;

namespace TaskManager.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "TaskManager";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Falta la cadena de conexión 'ConnectionStrings:{ConnectionStringName}'. " +
                "Configúrela con 'dotnet user-secrets' o con la variable de entorno ConnectionStrings__TaskManager.");
        }

        services.AddDbContext<TaskManagerDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserEFRepository>();
        services.AddScoped<IUserPhotoRepository, UserPhotoEFRepository>();
        services.AddScoped<ITaskRepository, TaskEFRepository>();

        services.AddScoped<ITaskReadRepository, TaskReadDapperRepository>();

        return services;
    }
}
