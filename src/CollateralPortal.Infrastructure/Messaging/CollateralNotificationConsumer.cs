using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CollateralPortal.Application.Common.Notifications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CollateralPortal.Infrastructure.Messaging;

/// <summary>
/// Background worker that drains the "collateral-notifications" queue and
/// forwards each message to the email notification service (RM/BM/AM/RH/
/// SG Head per the BRD). Decoupling publish (in the command handlers) from
/// send (here) means a slow/broken email provider never blocks a workflow step,
/// and matches the BRD's "all notifications are event-driven" requirement.
/// </summary>
public class CollateralNotificationConsumer : BackgroundService
{
    private const string QueueName = "collateral-notifications";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly ServiceBusClient _client;
    private readonly ILogger<CollateralNotificationConsumer> _logger;
    private ServiceBusProcessor? _processor;

    public CollateralNotificationConsumer(IOptions<ServiceBusOptions> options, ILogger<CollateralNotificationConsumer> logger)
    {
        _client = new ServiceBusClient(options.Value.ConnectionString);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = _client.CreateProcessor(QueueName, new ServiceBusProcessorOptions());

        _processor.ProcessMessageAsync += async args =>
        {
            var body = args.Message.Body.ToString();

            try
            {
                var notification = JsonSerializer.Deserialize<CollateralNotificationMessage>(body, JsonOptions);
                if (notification is not null)
                {
                    // TODO: forward to a real email provider (SendGrid/SMTP), one send per recipient role.
                    _logger.LogInformation(
                        "Email notification — To: {Recipients} | Subject: {Subject}\n{Body}",
                        string.Join(", ", notification.Recipients), notification.Subject, notification.Body);
                }
                else
                {
                    _logger.LogWarning("Notification message could not be deserialized: {Body}", body);
                }
            }
            catch (JsonException)
            {
                _logger.LogWarning("Notification message was not in the expected envelope shape: {Body}", body);
            }

            await args.CompleteMessageAsync(args.Message, stoppingToken);
        };

        _processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Service Bus processing error on {Queue}", QueueName);
            return Task.CompletedTask;
        };

        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // expected on shutdown
        }
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        if (_processor is not null)
            await _processor.StopProcessingAsync(ct);

        await base.StopAsync(ct);
    }
}
