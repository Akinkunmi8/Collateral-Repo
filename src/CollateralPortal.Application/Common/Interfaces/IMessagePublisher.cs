namespace CollateralPortal.Application.Common.Interfaces;

/// <summary>Thin abstraction over Azure Service Bus so Application never takes a direct SDK dependency.</summary>
public interface IMessagePublisher
{
    Task PublishAsync<T>(string queueOrTopicName, T message, CancellationToken ct = default) where T : class;
}
