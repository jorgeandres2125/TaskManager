namespace TaskManager.Domain.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
