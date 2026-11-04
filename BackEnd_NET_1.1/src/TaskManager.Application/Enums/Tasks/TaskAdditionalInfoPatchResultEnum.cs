namespace TaskManager.Application.Enums.Tasks;

/// <summary>Resultado de aplicar un cambio parcial al JSON de la tarea; dbo.usp_PatchTaskAdditionalInfo lo devuelve por su nombre.</summary>
public enum TaskAdditionalInfoPatchResultEnum
{
    Updated,
    TaskNotFound,
    TooManyTags,
    TooManyMetadataEntries,
}
