using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions.Tasks;
using TaskManager.Application.Enums.Tasks;
using TaskManager.Application.Models.Tasks;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Persistence.Converters;

namespace TaskManager.Infrastructure.Repositories.Dapper;

/// <summary>
/// Aplica cambios parciales a Tasks.AdditionalInfo con Dapper mediante el procedimiento dbo.usp_PatchTaskAdditionalInfo,
/// que usa JSON_MODIFY dentro de una transacción con la fila bloqueada. Las listas viajan como arrays u objetos JSON.
/// Usa la conexión del DbContext y su estrategia de ejecución; repetir el procedimiento tras un fallo transitorio
/// no duplica nada, porque cada cambio es idempotente.
/// </summary>
internal class TaskAdditionalInfoDapperRepository(TaskManagerDbContext context) : ITaskAdditionalInfoWriter
{
    private const string PatchProcedure = "dbo.usp_PatchTaskAdditionalInfo";

    public Task<TaskAdditionalInfoPatchResultEnum> PatchAsync(TaskAdditionalInfoPatch patch, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", patch.TaskId, DbType.Guid);
        parameters.Add("@Priority", patch.Priority?.ToString(), DbType.String, size: 10);
        parameters.Add("@ClearPriority", patch.ClearPriority, DbType.Boolean);
        parameters.Add("@EstimatedCompletionDate", patch.EstimatedCompletionDate?.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("@ClearEstimatedCompletionDate", patch.ClearEstimatedCompletionDate, DbType.Boolean);
        parameters.Add("@AddTags", JsonOrNull(patch.AddTags, patch.AddTags.Count), DbType.String, size: -1);
        parameters.Add("@RemoveTags", JsonOrNull(patch.RemoveTags, patch.RemoveTags.Count), DbType.String, size: -1);
        parameters.Add("@SetMetadata", JsonOrNull(patch.SetMetadata, patch.SetMetadata.Count), DbType.String, size: -1);
        parameters.Add("@RemoveMetadata", JsonOrNull(patch.RemoveMetadata, patch.RemoveMetadata.Count), DbType.String, size: -1);
        parameters.Add("@UpdatedAt", patch.UpdatedAt, DbType.DateTime2);
        parameters.Add("@MaxTags", TaskAdditionalInfo.MaxTags, DbType.Int32);
        parameters.Add("@MaxMetadataEntries", TaskAdditionalInfo.MaxMetadataEntries, DbType.Int32);

        return context.Database.CreateExecutionStrategy().ExecuteAsync(
            async token =>
            {
                var result = await context.Database.GetDbConnection().QuerySingleAsync<string>(new CommandDefinition(
                    PatchProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: token));

                return Enum.Parse<TaskAdditionalInfoPatchResultEnum>(result);
            },
            cancellationToken);
    }

    private static string? JsonOrNull<T>(T value, int count) =>
        count == 0 ? null : TaskAdditionalInfoJson.SerializeFragment(value);
}
