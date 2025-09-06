using ADMS.API.Configuration;
using ADMS.API.Services;
using ADMS.API.Validation;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;

namespace ADMS.API.Extensions;

/// <summary>
/// Extension methods for configuring services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures API-specific services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration validation and binding
        services.AddConfigurationServices(configuration);

        // Core API services
        services.AddCoreApiServices();

        // Validation services
        services.AddValidationServices();

        // Business services
        services.AddBusinessServices();

        // External services
        services.AddExternalServices(configuration);

        return services;
    }

    /// <summary>
    /// Adds configuration services and validates configuration sections.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddConfigurationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure and validate API settings
        services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"))
            .AddSingleton<IValidateOptions<ApiSettings>, ApiSettingsValidator>();

        // Configure and validate file storage settings
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"))
            .AddSingleton<IValidateOptions<FileStorageSettings>, FileStorageSettingsValidator>();

        // Configure and validate security settings
        services.Configure<SecuritySettings>(configuration.GetSection("Security"))
            .AddSingleton<IValidateOptions<SecuritySettings>, SecuritySettingsValidator>();

        // Configure and validate pagination settings
        services.Configure<PaginationSettings>(configuration.GetSection("Pagination"))
            .AddSingleton<IValidateOptions<PaginationSettings>, PaginationSettingsValidator>();

        // Validate all options on startup
        services.AddOptions<ApiSettings>()
            .BindConfiguration("ApiSettings")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<FileStorageSettings>()
            .BindConfiguration("FileStorage")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Adds core API services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddCoreApiServices(this IServiceCollection services)
    {
        // Property services for sorting and data shaping
        services.AddScoped<IPropertyMappingService, PropertyMappingService>();
        services.AddScoped<IPropertyCheckerService, PropertyCheckerService>();

        // Custom services for specific functionality
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IFileProcessingService, FileProcessingService>();
        services.AddScoped<IAuditService, AuditService>();

        // Caching services
        services.AddScoped<ICacheService, MemoryCacheService>();

        return services;
    }

    /// <summary>
    /// Adds validation services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        // Core validation service
        services.AddScoped<IValidationService, ValidationService>();

        // Specific validators
        services.AddScoped<IDocumentValidator, DocumentValidator>();
        services.AddScoped<IMatterValidator, MatterValidator>();
        services.AddScoped<IRevisionValidator, RevisionValidator>();
        services.AddScoped<IFileValidator, FileValidator>();

        // Custom validation attributes
        services.AddScoped<FileExtensionValidationAttribute>();
        services.AddScoped<FileSizeValidationAttribute>();

        return services;
    }

    /// <summary>
    /// Adds business services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Business logic services
        services.AddScoped<IDocumentManagementService, DocumentManagementService>();
        services.AddScoped<IMatterManagementService, MatterManagementService>();
        services.AddScoped<IRevisionManagementService, RevisionManagementService>();

        // Workflow services
        services.AddScoped<IDocumentWorkflowService, DocumentWorkflowService>();
        services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();

        // Notification services
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }

    /// <summary>
    /// Adds external services and integrations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Virus scanning service
        services.Configure<VirusScannerSettings>(configuration.GetSection("VirusScanner"));
        services.AddScoped<IVirusScanner, ClamAvVirusScanner>();

        // File storage service
        services.AddScoped<IFileStorage, FileSystemFileStorage>();

        // Document conversion services
        services.AddScoped<IPdfConversionService, SyncfusionPdfConversionService>();
        services.AddScoped<IDocumentPreviewService, DocumentPreviewService>();

        // Email service (if needed)
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.AddScoped<IEmailService, EmailService>();

        // HTTP clients for external APIs
        services.AddHttpClient<IExternalApiClient, ExternalApiClient>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ExternalApi:BaseUrl") ?? "");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    /// <summary>
    /// Adds health check services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready", "database" })
            .AddCheck<FileStorageHealthCheck>("file-storage", tags: new[] { "ready", "storage" })
            .AddCheck<VirusScannerHealthCheck>("virus-scanner", tags: new[] { "ready", "security" })
            .AddCheck<ExternalApiHealthCheck>("external-api", tags: new[] { "ready", "external" });

        return services;
    }
}

/// <summary>
/// Configuration settings for the API.
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// Gets or sets the API name.
    /// </summary>
    [Required]
    public string Name { get; set; } = "ADMS API";

    /// <summary>
    /// Gets or sets the API version.
    /// </summary>
    [Required]
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets the maximum request timeout in seconds.
    /// </summary>
    [Range(1, 300)]
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the default page size for paginated responses.
    /// </summary>
    [Range(1, 100)]
    public int DefaultPageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum page size for paginated responses.
    /// </summary>
    [Range(1, 1000)]
    public int MaxPageSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets a value indicating whether detailed error responses are enabled.
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether Swagger is enabled in production.
    /// </summary>
    public bool EnableSwaggerInProduction { get; set; } = false;
}

/// <summary>
/// Configuration settings for file storage.
/// </summary>
public class FileStorageSettings
{
    /// <summary>
    /// Gets or sets the base path for file storage.
    /// </summary>
    [Required]
    public string BasePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum file size in bytes.
    /// </summary>
    [Range(1, long.MaxValue)]
    public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024; // 50 MB

    /// <summary>
    /// Gets or sets the allowed file extensions.
    /// </summary>
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether virus scanning is enabled.
    /// </summary>
    public bool EnableVirusScanning { get; set; } = true;

    /// <summary>
    /// Gets or sets the temporary file retention period in hours.
    /// </summary>
    [Range(1, 168)] // 1 hour to 1 week
    public int TempFileRetentionHours { get; set; } = 24;
}

/// <summary>
/// Configuration settings for security.
/// </summary>
public class SecuritySettings
{
    /// <summary>
    /// Gets or sets a value indicating whether HTTPS is required.
    /// </summary>
    public bool RequireHttps { get; set; } = true;

    /// <summary>
    /// Gets or sets the allowed CORS origins.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the rate limit per minute per user.
    /// </summary>
    [Range(1, 10000)]
    public int RateLimitPerMinute { get; set; } = 100;

    /// <summary>
    /// Gets or sets the JWT token expiration time in minutes.
    /// </summary>
    [Range(1, 43200)] // 1 minute to 30 days
    public int JwtExpirationMinutes { get; set; } = 60;
}

/// <summary>
/// Configuration settings for pagination.
/// </summary>
public class PaginationSettings
{
    /// <summary>
    /// Gets or sets the default page size.
    /// </summary>
    [Range(1, 100)]
    public int DefaultPageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum page size.
    /// </summary>
    [Range(1, 1000)]
    public int MaxPageSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets a value indicating whether page size validation is strict.
    /// </summary>
    public bool StrictPageSizeValidation { get; set; } = true;
}

/// <summary>
/// Configuration settings for virus scanner.
/// </summary>
public class VirusScannerSettings
{
    /// <summary>
    /// Gets or sets the ClamAV host.
    /// </summary>
    [Required]
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the ClamAV port.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 3310;

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets a value indicating whether virus scanning is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Configuration settings for email service.
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Gets or sets the SMTP host.
    /// </summary>
    [Required]
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP port.
    /// </summary>
    [Range(1, 65535)]
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Gets or sets a value indicating whether SSL is enabled.
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Gets or sets the sender email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender display name.
    /// </summary>
    public string SenderName { get; set; } = "ADMS System";
}