using ADMS.Domain.Common;
using ADMS.Domain.Events;
using ADMS.Domain.Services;
using ADMS.Infrastructure.EventHandlers;
using ADMS.Infrastructure.EventHandlers.Composite;
using ADMS.Infrastructure.EventHandlers.Document;
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

        // Register domain event handlers
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
        // Individual audit handlers
        services.AddDomainEventHandler<DocumentCreatedDomainEvent, DocumentCreatedAuditHandler>();
        services.AddDomainEventHandler<DocumentCheckedOutDomainEvent, DocumentCheckedOutAuditHandler>();
        services.AddDomainEventHandler<DocumentCheckedInDomainEvent, DocumentCheckedInAuditHandler>();
        services.AddDomainEventHandler<DocumentDeletedDomainEvent, DocumentDeletedAuditHandler>();

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
        // Simple registration
        services.AddADMSDomain();

        // Or more granular control
        services.AddDomainServices()
                .AddDomainEvents()
                .ConfigureDomainEventHandlers()
                    .AddHandler<DocumentCreatedDomainEvent, DocumentCreatedAuditHandler>()
                    .AddHandler<DocumentDeletedDomainEvent, DocumentDeletedAuditHandler>()
                    .Services;

        // Register the DbContext with domain events support
        // services.AddDbContext<AdmsDbContext>((provider, options) =>
        // {
        //     options.UseSqlServer(connectionString);
        //     // The domain event dispatcher will be injected automatically
        // });
    }
}