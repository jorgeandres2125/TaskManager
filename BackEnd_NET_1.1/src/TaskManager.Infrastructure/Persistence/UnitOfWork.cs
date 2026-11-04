using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Exceptions;
using TaskManager.Domain.Interfaces.Persistence;

namespace TaskManager.Infrastructure.Persistence;

/// <summary>
/// Confirma los cambios y traduce las violaciones de restricciones de SQL Server en conflictos.
/// Cubre las condiciones de carrera que las validaciones previas no pueden evitar.
/// </summary>
internal class UnitOfWork(TaskManagerDbContext context) : IUnitOfWork
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;
    private const int ForeignKeyViolation = 547;

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException sqlException)
        {
            throw sqlException.Number switch
            {
                UniqueIndexViolation or UniqueConstraintViolation =>
                    new ConflictException("Ya existe un registro con los mismos datos únicos."),
                ForeignKeyViolation =>
                    new ConflictException("El registro relacionado no existe o está en uso."),
                _ => exception,
            };
        }
    }
}
