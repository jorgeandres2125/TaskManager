namespace TaskManager.Domain.Entities.Base;

/// <summary>
/// Base de todas las entidades del modelo: identificador GUID y fecha de creación obligatoria (UTC).
/// </summary>
public abstract class Entity
{
    public Guid Id { get; private init; }

    public DateTime CreatedAt { get; private init; }

    protected Entity(Guid id, DateTime createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador no puede ser vacío.", nameof(id));
        }

        if (createdAt.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("La fecha de creación debe estar en UTC.", nameof(createdAt));
        }

        Id = id;
        CreatedAt = createdAt;
    }

    protected Entity()
    {
    }

    protected static Guid NewId() => Guid.CreateVersion7();
}
