using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CollateralPortal.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CollateralPortal.Infrastructure.Messaging;

/// <summary>
/// Publishes to Azure Service Bus queues/topics. Backed by the official
/// Azure Service Bus emulator in docker-compose for local dev, and the
/// real namespace in Azure — same connection-string-based client either way.
/// </summary>
public class ServiceBusMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusMessagePublisher> _logger;

    public ServiceBusMessagePublisher(IOptions<ServiceBusOptions> options, ILogger<ServiceBusMessagePublisher> logger)
    {
        _logger = logger;
        _client = new ServiceBusClient(options.Value.ConnectionString);
    }

    public async Task PublishAsync<T>(string queueOrTopicName, T message, CancellationToken ct = default) where T : class
    {
        await using var sender = _client.CreateSender(queueOrTopicName);

        var body = JsonSerializer.Serialize(message);
        var sbMessage = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            Subject = typeof(T).Name
        };

        await sender.SendMessageAsync(sbMessage, ct);
        _logger.LogInformation("Published {MessageType} to {Queue}", typeof(T).Name, queueOrTopicName);
    }

    public async ValueTask DisposeAsync() => await _client.DisposeAsync();
}
