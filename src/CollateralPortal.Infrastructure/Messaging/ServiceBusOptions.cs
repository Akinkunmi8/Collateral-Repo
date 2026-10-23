namespace CollateralPortal.Infrastructure.Messaging;

public class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    /// <summary>
    /// Connection string. In docker-compose this points at the local Azure
    /// Service Bus emulator; in Azure it's the real namespace connection
    /// string (kept in Key Vault, not appsettings, for real environments).
    /// </summary>
    public string ConnectionString { get; set; } = default!;
}
