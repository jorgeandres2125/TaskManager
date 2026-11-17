using TaskManager.Application.Abstractions.Tasks;
using TaskManager.Application.Enums.Tasks;
using TaskManager.Application.Models.Tasks;

namespace TaskManager.Tests.TestDoubles;

/// <summary>Guarda el último cambio recibido y devuelve el resultado configurado, sin tocar ninguna base de datos.</summary>
internal class RecordingTaskAdditionalInfoWriter : ITaskAdditionalInfoWriter
{
    public TaskAdditionalInfoPatchResultEnum Result { get; set; } = TaskAdditionalInfoPatchResultEnum.Updated;

    public TaskAdditionalInfoPatch? LastPatch { get; private set; }

    public Task<TaskAdditionalInfoPatchResultEnum> PatchAsync(TaskAdditionalInfoPatch patch, CancellationToken cancellationToken)
    {
        LastPatch = patch;
        return Task.FromResult(Result);
    }
}
