using ADMS.Domain.Common;
using ADMS.Domain.Events;
using ADMS.Domain.Services;
using ADMS.Infrastructure.EventHandlers.Composite;
using ADMS.Infrastructure.EventHandlers.Document;
using ADMS.Infrastructure.EventHandlers.Matter;
using ADMS.Infrastructure.Events;

using Microsoft.Extensions.DependencyInjection;

namespace ADMS.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering ADMS domain and infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ADMS domain services, domain events, and infrastructure services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddADMSDomain(this IServiceCollection services)
    {
        // Register domain services
        services.AddDomainServices();

        // Register domain events infrastructure
        services.AddDomainEvents();

        // Register all domain event handlers
        services.AddDomainEventHandlers();

        return services;
    }

    /// <summary>
    /// Registers domain services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IDocumentValidationService, DocumentValidationService>();

        // Add other domain services as they are created:
        // services.AddScoped<IMatterValidationService, MatterValidationService>();
        // services.AddScoped<IRevisionValidationService, RevisionValidationService>();
        // services.AddScoped<IUserValidationService, UserValidationService>();

        return services;
    }

    /// <summary>
    /// Registers all domain event handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services)
    {
        // Document audit handlers
        services.AddDocumentEventHandlers();

        // Matter audit handlers
        services.AddMatterEventHandlers();

        // Future: Add other entity handlers
        // services.AddRevisionEventHandlers();
        // services.AddUserEventHandlers();

        return services;
    }

    /// <summary>
    /// Registers document-specific event handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDocumentEventHandlers(this IServiceCollection services)
    {
        // Individual document audit handlers
        services.AddDomainEventHandler<DocumentCreatedDomainEvent, DocumentCreatedAuditHandler>();
        services.AddDomainEventHandler<DocumentCheckedOutDomainEvent, DocumentCheckedOutAuditHandler>();
        services.AddDomainEventHandler<DocumentCheckedInDomainEvent, DocumentCheckedInAuditHandler>();
        services.AddDomainEventHandler<DocumentDeletedDomainEvent, DocumentDeletedAuditHandler>();
        services.AddDomainEventHandler<DocumentRestoredDomainEvent, DocumentRestoredAuditHandler>();

        // Composite audit trail manager (can handle multiple event types)
        services.AddScoped<DocumentAuditTrailManager>();
        services.AddScoped<IDomainEventHandler<DocumentCreatedDomainEvent>>(provider =>
            provider.GetRequiredService<DocumentAuditTrailManager>());
        services.AddScoped<IDomainEventHandler<DocumentCheckedOutDomainEvent>>(provider =>
            provider.GetRequiredService<DocumentAuditTrailManager>());
        services.AddScoped<IDomainEventHandler<DocumentCheckedInDomainEvent>>(provider =>
            provider.GetRequiredService<DocumentAuditTrailManager>());
        services.AddScoped<IDomainEventHandler<DocumentDeletedDomainEvent>>(provider =>
            provider.GetRequiredService<DocumentAuditTrailManager>());

        return services;
    }

    /// <summary>
    /// Registers matter-specific event handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMatterEventHandlers(this IServiceCollection services)
    {
        // Individual matter audit handlers
        services.AddDomainEventHandler<MatterCreatedDomainEvent, MatterCreatedAuditHandler>();
        services.AddDomainEventHandler<MatterArchivedDomainEvent, MatterArchivedAuditHandler>();
        services.AddDomainEventHandler<MatterDeletedDomainEvent, MatterDeletedAuditHandler>();

        // Future matter event handlers
        // services.AddDomainEventHandler<MatterRestoredDomainEvent, MatterRestoredAuditHandler>();
        // services.AddDomainEventHandler<MatterUnarchivedDomainEvent, MatterUnarchivedAuditHandler>();
        // services.AddDomainEventHandler<MatterUpdatedDomainEvent, MatterUpdatedAuditHandler>();

        return services;
    }

    /// <summary>
    /// Registers domain event handlers using a more convenient fluent syntax.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>A configuration object for adding handlers.</returns>
    public static DomainEventHandlerConfiguration ConfigureDomainEventHandlers(this IServiceCollection services)
    {
        return new DomainEventHandlerConfiguration(services);
    }
}

/// <summary>
/// Fluent configuration for domain event handlers.
/// </summary>
public class DomainEventHandlerConfiguration
{
    private readonly IServiceCollection _services;

    internal DomainEventHandlerConfiguration(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Adds a domain event handler for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The domain event type.</typeparam>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <returns>The configuration object for chaining.</returns>
    public DomainEventHandlerConfiguration AddHandler<TEvent, THandler>()
        where TEvent : class, IDomainEvent
        where THandler : class, IDomainEventHandler<TEvent>
    {
        _services.AddDomainEventHandler<TEvent, THandler>();
        return this;
    }

    /// <summary>
    /// Completes the configuration and returns the service collection.
    /// </summary>
    /// <returns>The service collection.</returns>
    public IServiceCollection Services => _services;
}

/// <summary>
/// Example usage in Program.cs or Startup.cs
/// </summary>
public static class ExampleUsage
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Simple registration - registers everything
        services.AddADMSDomain();

        // Or more granular control
        services.AddDomainServices()
                .AddDomainEvents()
                .AddDocumentEventHandlers()
                .AddMatterEventHandlers();

        // Or fluent configuration for specific handlers
        services.ConfigureDomainEventHandlers()
                .AddHandler<DocumentCreatedDomainEvent, DocumentCreatedAuditHandler>()
                .AddHandler<MatterCreatedDomainEvent, MatterCreatedAuditHandler>()
                .AddHandler<MatterArchivedDomainEvent, MatterArchivedAuditHandler>()
                .AddHandler<MatterDeletedDomainEvent, MatterDeletedAuditHandler>()
                .Services;
    }
}