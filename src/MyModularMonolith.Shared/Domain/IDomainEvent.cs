using MediatR;

namespace MyModularMonolith.Shared.Domain;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
