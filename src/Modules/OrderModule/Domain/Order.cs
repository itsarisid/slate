namespace Alphabet.Modules.OrderModule.Domain;

/// <summary>
/// Represents a lightweight example bounded context aggregate.
/// </summary>
public sealed class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid CustomerId { get; init; }

    public decimal Total { get; init; }
}
