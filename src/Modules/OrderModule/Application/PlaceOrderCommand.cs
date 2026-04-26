using MediatR;

namespace Alphabet.Modules.OrderModule.Application;

/// <summary>
/// Demonstrates a self-contained command within a module.
/// </summary>
public sealed record PlaceOrderCommand(Guid CustomerId, decimal Total) : IRequest<Guid>;
