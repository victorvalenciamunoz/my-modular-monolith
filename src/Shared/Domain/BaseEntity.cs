    using System.ComponentModel.DataAnnotations;

namespace MyModularMonolith.Shared.Domain;

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected BaseEntity()
    {
        // CreatedAt se establecer� en el constructor espec�fico o mediante m�todo
    }

    protected void SetCreated(DateTime createdAt)
    {
        CreatedAt = createdAt;
    }

    protected void SetUpdated(DateTime updatedAt)
    {
        UpdatedAt = updatedAt;
    }
}