using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.Behaviors;
using TaskManager.Application.DTOs.Pagination;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.DTOs.Users;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Application.UseCases.Handlers.Tasks;
using TaskManager.Application.UseCases.Handlers.Users;
using TaskManager.Application.UseCases.Queries.Tasks;
using TaskManager.Application.UseCases.Queries.Users;

namespace TaskManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("es");
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services
            .AddRequestHandler<CreateUserCommand, UserDto, CreateUserCommandHandler>()
            .AddRequestHandler<GetUsersQuery, PagedResultDto<UserDto>, GetUsersQueryHandler>()
            .AddRequestHandler<UpdateUserCommand, UserDto, UpdateUserCommandHandler>()
            .AddRequestHandler<GetUserByIdQuery, UserDto, GetUserByIdQueryHandler>()
            .AddRequestHandler<UploadUserPhotoCommand, UserDto, UploadUserPhotoCommandHandler>()
            .AddRequestHandler<DeleteUserPhotoCommand, UserDto, DeleteUserPhotoCommandHandler>()
            .AddRequestHandler<GetUserPhotoQuery, UserPhotoFileDto, GetUserPhotoQueryHandler>()
            .AddRequestHandler<CreateTaskCommand, TaskDto, CreateTaskCommandHandler>()
            .AddRequestHandler<GetTasksQuery, PagedResultDto<TaskDto>, GetTasksQueryHandler>()
            .AddRequestHandler<GetTaskByIdQuery, TaskDto, GetTaskByIdQueryHandler>()
            .AddRequestHandler<UpdateTaskCommand, TaskDto, UpdateTaskCommandHandler>()
            .AddRequestHandler<PatchTaskAdditionalInfoCommand, TaskDto, PatchTaskAdditionalInfoCommandHandler>()
            .AddRequestHandler<ChangeTaskStatusCommand, TaskDto, ChangeTaskStatusCommandHandler>();

        return services;
    }

    private static IServiceCollection AddRequestHandler<TRequest, TResponse, THandler>(this IServiceCollection services)
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IRequestHandler<TRequest, TResponse>>(provider =>
            new ValidationBehavior<TRequest, TResponse>(
                provider.GetRequiredService<THandler>(),
                provider.GetServices<IValidator<TRequest>>()));

        return services;
    }
}
