namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a directed dependency between tasks.
/// </summary>
public sealed class TaskDependency : BaseEntity
{
    public Guid ProductivityTaskId { get; private set; }

    public Guid DependsOnTaskId { get; private set; }

    private TaskDependency()
    {
    }

    private TaskDependency(Guid productivityTaskId, Guid dependsOnTaskId)
    {
        ProductivityTaskId = productivityTaskId;
        DependsOnTaskId = dependsOnTaskId;
    }
    /// <summary>
    /// Create.
    /// </summary>

    public static TaskDependency Create(Guid productivityTaskId, Guid dependsOnTaskId)
        => new(productivityTaskId, dependsOnTaskId);
}
