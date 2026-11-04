using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions.Tasks;
using TaskManager.Application.Enums.Sorting;
using TaskManager.Application.Models.Pagination;
using TaskManager.Application.Models.Tasks;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Enums.Tasks;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Persistence.Converters;

namespace TaskManager.Infrastructure.Repositories.Dapper;

/// <summary>
/// Lee tareas con Dapper mediante el procedimiento dbo.usp_GetTasks, que resuelve en SQL Server el filtro por usuario,
/// estado, prioridad (JSON_VALUE) y etiqueta (OPENJSON), el orden por fecha y la proyección del JSON (JSON_QUERY).
/// Usa la conexión del DbContext y su estrategia de ejecución, para conservar los reintentos ante fallos transitorios.
/// </summary>
internal class TaskReadDapperRepository(TaskManagerDbContext context) : ITaskReadRepository
{
    private const string GetTasksProcedure = "dbo.usp_GetTasks";

    public Task<PagedList<TaskReadModel>> SearchAsync(TaskSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", criteria.TaskId, DbType.Guid);
        parameters.Add("@UserId", criteria.UserId, DbType.Guid);
        parameters.Add("@Status", criteria.Status?.ToString(), DbType.String, size: 20);
        parameters.Add("@Priority", criteria.Priority?.ToString(), DbType.String, size: 10);
        parameters.Add("@Tag", criteria.Tag, DbType.String, size: TaskAdditionalInfo.TagMaxLength);
        parameters.Add("@SortDescending", criteria.SortDirection == SortDirectionEnum.Desc, DbType.Boolean);
        parameters.Add("@PageNumber", criteria.Page?.Page, DbType.Int32);
        parameters.Add("@PageSize", criteria.Page?.PageSize, DbType.Int32);

        return context.Database.CreateExecutionStrategy().ExecuteAsync(
            async token =>
            {
                using var results = await context.Database.GetDbConnection().QueryMultipleAsync(new CommandDefinition(
                    GetTasksProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: token));

                var totalCount = await results.ReadSingleAsync<int>();
                var rows = await results.ReadAsync<TaskRow>();

                return new PagedList<TaskReadModel>(rows.Select(ToReadModel).ToArray(), totalCount);
            },
            cancellationToken);
    }

    private static TaskReadModel ToReadModel(TaskRow row) => new(
        row.Id,
        row.Number,
        row.Title,
        Enum.Parse<TaskItemStatusEnum>(row.Status),
        row.UserId,
        row.UserName,
        UtcDateTimeConverter.AsUtc(row.CreatedAt),
        row.UpdatedAt is { } updatedAt ? UtcDateTimeConverter.AsUtc(updatedAt) : null,
        TaskAdditionalInfo.Create(
            row.Priority is null ? null : Enum.Parse<TaskPriorityEnum>(row.Priority),
            row.EstimatedCompletionDate is { } date ? DateOnly.FromDateTime(date) : null,
            TaskAdditionalInfoJson.DeserializeFragment<string[]>(row.Tags),
            TaskAdditionalInfoJson.DeserializeFragment<Dictionary<string, string>>(row.Metadata)));

    /// <summary>Fila devuelta por dbo.usp_GetTasks (los nombres coinciden con las columnas).</summary>
    internal class TaskRow
    {
        public Guid Id { get; init; }

        public long Number { get; init; }

        public string Title { get; init; } = null!;

        public string Status { get; init; } = null!;

        public Guid UserId { get; init; }

        public string UserName { get; init; } = null!;

        public DateTime CreatedAt { get; init; }

        public DateTime? UpdatedAt { get; init; }

        public string? Priority { get; init; }

        public DateTime? EstimatedCompletionDate { get; init; }

        public string? Tags { get; init; }

        public string? Metadata { get; init; }
    }
}
