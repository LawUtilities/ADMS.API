using ADMS.API.Extensions;
using ADMS.API.Services.Caching;

using Microsoft.Extensions.Options;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ADMS.API.Configuration;

/// <summary>
/// Validator for API settings configuration.
/// </summary>
public class ApiSettingsValidator : IValidateOptions<ApiSettings>
{
    /// <summary>
    /// Validates the API settings configuration.
    /// </summary>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="options">The options instance to validate.</param>
    /// <returns>The validation result.</returns>
    public ValidateOptionsResult Validate(string? name, ApiSettings options)
    {
        var failures = new List<string>();

        // Validate required properties
        if (string.IsNullOrWhiteSpace(options.Name))
        {
            failures.Add("API name cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(options.Version))
        {
            failures.Add("API version cannot be null or empty.");
        }

        // Validate ranges
        if (options.RequestTimeoutSeconds <= 0 || options.RequestTimeoutSeconds > 300)
        {
            failures.Add("Request timeout must be between 1 and 300 seconds.");
        }

        if (options.DefaultPageSize <= 0 || options.DefaultPageSize > 100)
        {
            failures.Add("Default page size must be between 1 and 100.");
        }

        if (options.MaxPageSize < options.DefaultPageSize || options.MaxPageSize > 1000)
        {
            failures.Add("Max page size must be greater than or equal to default page size and not exceed 1000.");
        }

        // Validate version format
        if (!IsValidSemanticVersion(options.Version))
        {
            failures.Add("API version must be a valid semantic version (e.g., '1.0', '1.0.0').");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    /// <summary>
    /// Validates if a version string follows semantic versioning.
    /// </summary>
    /// <param name="version">The version string to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    private static bool IsValidSemanticVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return false;

        var parts = version.Split('.');
        if (parts.Length < 2 || parts.Length > 3)
            return false;

        return parts.All(part => int.TryParse(part, out var num) && num >= 0);
    }
}

/// <summary>
/// Validator for file storage settings configuration.
/// </summary>
public class FileStorageSettingsValidator : IValidateOptions<FileStorageSettings>
{
    /// <summary>
    /// Validates the file storage settings configuration.
    /// </summary>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="options">The options instance to validate.</param>
    /// <returns>The validation result.</returns>
    public ValidateOptionsResult Validate(string? name, FileStorageSettings options)
    {
        var failures = new List<string>();

        // Validate base path
        if (string.IsNullOrWhiteSpace(options.BasePath))
        {
            failures.Add("Base path cannot be null or empty.");
        }
        else
        {
            try
            {
                // Validate that the path is valid
                var fullPath = Path.GetFullPath(options.BasePath);

                // Check if it's a valid directory path
                if (!Path.IsPathFullyQualified(fullPath))
                {
                    failures.Add("Base path must be a fully qualified path.");
                }
            }
            catch (Exception ex)
            {
                failures.Add($"Invalid base path: {ex.Message}");
            }
        }

        // Validate file size
        if (options.MaxFileSizeBytes <= 0)
        {
            failures.Add("Maximum file size must be greater than zero.");
        }

        const long maxAllowedFileSize = 1024L * 1024L * 1024L; // 1 GB
        if (options.MaxFileSizeBytes > maxAllowedFileSize)
        {
            failures.Add($"Maximum file size cannot exceed {maxAllowedFileSize:N0} bytes (1 GB).");
        }

        // Validate allowed extensions
        if (options.AllowedExtensions == null || options.AllowedExtensions.Length == 0)
        {
            failures.Add("At least one allowed file extension must be specified.");
        }
        else
        {
            foreach (var extension in options.AllowedExtensions)
            {
                if (string.IsNullOrWhiteSpace(extension))
                {
                    failures.Add("File extensions cannot be null or empty.");
                    break;
                }

                if (!extension.StartsWith('.'))
                {
                    failures.Add($"File extension '{extension}' must start with a dot.");
                }

                if (extension.Length < 2)
                {
                    failures.Add($"File extension '{extension}' is too short.");
                }

                if (extension.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
                {
                    failures.Add($"File extension '{extension}' contains invalid characters.");
                }
            }
        }

        // Validate retention period
        if (options.TempFileRetentionHours <= 0 || options.TempFileRetentionHours > 168) // Max 1 week
        {
            failures.Add("Temporary file retention hours must be between 1 and 168 (1 week).");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Validator for security settings configuration.
/// </summary>
public class SecuritySettingsValidator : IValidateOptions<SecuritySettings>
{
    /// <summary>
    /// Validates the security settings configuration.
    /// </summary>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="options">The options instance to validate.</param>
    /// <returns>The validation result.</returns>
    public ValidateOptionsResult Validate(string? name, SecuritySettings options)
    {
        var failures = new List<string>();

        // Validate CORS origins
        if (options.AllowedOrigins?.Length > 0)
        {
            foreach (var origin in options.AllowedOrigins)
            {
                if (string.IsNullOrWhiteSpace(origin))
                {
                    failures.Add("CORS origins cannot be null or empty.");
                    continue;
                }

                if (origin != "*" && !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    failures.Add($"Invalid CORS origin format: '{origin}'.");
                }
            }
        }

        // Validate rate limiting
        if (options.RateLimitPerMinute <= 0 || options.RateLimitPerMinute > 10000)
        {
            failures.Add("Rate limit per minute must be between 1 and 10,000.");
        }

        // Validate JWT expiration
        if (options.JwtExpirationMinutes <= 0 || options.JwtExpirationMinutes > 43200) // Max 30 days
        {
            failures.Add("JWT expiration minutes must be between 1 and 43,200 (30 days).");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Validator for pagination settings configuration.
/// </summary>
public class PaginationSettingsValidator : IValidateOptions<PaginationSettings>
{
    /// <summary>
    /// Validates the pagination settings configuration.
    /// </summary>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="options">The options instance to validate.</param>
    /// <returns>The validation result.</returns>
    public ValidateOptionsResult Validate(string? name, PaginationSettings options)
    {
        var failures = new List<string>();

        // Validate default page size
        if (options.DefaultPageSize <= 0 || options.DefaultPageSize > 100)
        {
            failures.Add("Default page size must be between 1 and 100.");
        }

        // Validate max page size
        if (options.MaxPageSize <= 0 || options.MaxPageSize > 1000)
        {
            failures.Add("Maximum page size must be between 1 and 1,000.");
        }

        // Ensure max page size is not smaller than default
        if (options.MaxPageSize < options.DefaultPageSize)
        {
            failures.Add("Maximum page size must be greater than or equal to default page size.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Comprehensive configuration validation service.
/// </summary>
public interface IConfigurationValidationService
{
    /// <summary>
    /// Validates all configuration sections.
    /// </summary>
    /// <returns>A validation result containing any configuration errors.</returns>
    Task<ConfigurationValidationResult> ValidateAllAsync();

    /// <summary>
    /// Validates a specific configuration section.
    /// </summary>
    /// <typeparam name="T">The configuration type to validate.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>A validation result for the specific section.</returns>
    Task<ConfigurationValidationResult> ValidateSectionAsync<T>(string sectionName) where T : class, new();

    /// <summary>
    /// Gets configuration health status.
    /// </summary>
    /// <returns>Configuration health information.</returns>
    ConfigurationHealth GetConfigurationHealth();
}

/// <summary>
/// Implementation of configuration validation service.
/// </summary>
public class ConfigurationValidationService : IConfigurationValidationService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConfigurationValidationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="serviceProvider">The service provider for resolving validators.</param>
    /// <param name="logger">The logger instance.</param>
    public ConfigurationValidationService(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<ConfigurationValidationService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ConfigurationValidationResult> ValidateAllAsync()
    {
        var result = new ConfigurationValidationResult();

        try
        {
            // Define all configuration sections to validate
            var validations = new[]
            {
                ValidateSectionAsync<ApiSettings>("ApiSettings"),
                ValidateSectionAsync<FileStorageSettings>("FileStorage"),
                ValidateSectionAsync<SecuritySettings>("Security"),
                ValidateSectionAsync<PaginationSettings>("Pagination"),
                ValidateSectionAsync<VirusScannerSettings>("VirusScanner"),
                ValidateSectionAsync<EmailSettings>("Email"),
                ValidateSectionAsync<CacheSettings>("Cache"),
                ValidateConnectionStringsAsync(),
                ValidateRequiredEnvironmentVariablesAsync()
            };

            var validationResults = await Task.WhenAll(validations);

            // Aggregate all validation results
            foreach (var validationResult in validationResults)
            {
                result.Errors.AddRange(validationResult.Errors);
                result.Warnings.AddRange(validationResult.Warnings);
            }

            result.IsValid = result.Errors.Count == 0;

            _logger.LogInformation(
                "Configuration validation completed. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}",
                result.IsValid, result.Errors.Count, result.Warnings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during configuration validation");
            result.Errors.Add($"Configuration validation failed: {ex.Message}");
            result.IsValid = false;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<ConfigurationValidationResult> ValidateSectionAsync<T>(string sectionName) where T : class, new()
    {
        var result = new ConfigurationValidationResult();

        try
        {
            // Check if section exists
            var section = _configuration.GetSection(sectionName);
            if (!section.Exists())
            {
                result.Warnings.Add($"Configuration section '{sectionName}' does not exist.");
                return result;
            }

            // Bind and validate the configuration
            var options = new T();
            section.Bind(options);

            // Perform data annotation validation
            var validationContext = new ValidationContext(options);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(options, validationContext, validationResults, validateAllProperties: true))
            {
                foreach (var validationResult in validationResults)
                {
                    result.Errors.Add($"{sectionName}: {validationResult.ErrorMessage}");
                }
            }

            // Use custom validator if available
            var validatorType = typeof(IValidateOptions<>).MakeGenericType(typeof(T));
            var validator = _serviceProvider.GetService(validatorType) as IValidateOptions<T>;

            if (validator != null)
            {
                var validateResult = validator.Validate(sectionName, options);
                if (validateResult.Failed)
                {
                    foreach (var failure in validateResult.Failures)
                    {
                        result.Errors.Add($"{sectionName}: {failure}");
                    }
                }
            }

            // Additional custom validations
            await PerformCustomValidationsAsync(sectionName, options, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating configuration section: {SectionName}", sectionName);
            result.Errors.Add($"Error validating section '{sectionName}': {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc />
    public ConfigurationHealth GetConfigurationHealth()
    {
        var health = new ConfigurationHealth
        {
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            // Check critical sections
            var criticalSections = new[] { "ConnectionStrings", "ApiSettings", "FileStorage" };

            foreach (var sectionName in criticalSections)
            {
                var section = _configuration.GetSection(sectionName);
                var sectionHealth = new ConfigurationSectionHealth
                {
                    SectionName = sectionName,
                    Exists = section.Exists(),
                    HasValues = section.GetChildren().Any()
                };

                if (sectionHealth.Exists && sectionHealth.HasValues)
                {
                    sectionHealth.Status = ConfigurationSectionStatus.Healthy;
                }
                else if (sectionHealth.Exists)
                {
                    sectionHealth.Status = ConfigurationSectionStatus.Warning;
                    sectionHealth.Issues.Add("Section exists but has no values");
                }
                else
                {
                    sectionHealth.Status = ConfigurationSectionStatus.Error;
                    sectionHealth.Issues.Add("Section does not exist");
                }

                health.Sections.Add(sectionHealth);
            }

            // Check environment-specific settings
            CheckEnvironmentSpecificSettings(health);

            // Determine overall health
            if (health.Sections.Any(s => s.Status == ConfigurationSectionStatus.Error))
            {
                health.OverallStatus = ConfigurationStatus.Unhealthy;
            }
            else if (health.Sections.Any(s => s.Status == ConfigurationSectionStatus.Warning))
            {
                health.OverallStatus = ConfigurationStatus.Warning;
            }
            else
            {
                health.OverallStatus = ConfigurationStatus.Healthy;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking configuration health");
            health.OverallStatus = ConfigurationStatus.Unhealthy;
        }

        return health;
    }

    #region Private Helper Methods

    /// <summary>
    /// Validates connection strings configuration.
    /// </summary>
    /// <returns>Validation result for connection strings.</returns>
    private async Task<ConfigurationValidationResult> ValidateConnectionStringsAsync()
    {
        var result = new ConfigurationValidationResult();

        try
        {
            var connectionStrings = _configuration.GetSection("ConnectionStrings");
            if (!connectionStrings.Exists())
            {
                result.Errors.Add("ConnectionStrings section is missing.");
                return result;
            }

            var defaultConnection = connectionStrings["DefaultConnection"];
            if (string.IsNullOrWhiteSpace(defaultConnection))
            {
                result.Errors.Add("DefaultConnection string is missing or empty.");
            }
            else
            {
                // Validate connection string format (basic check)
                if (!IsValidConnectionString(defaultConnection))
                {
                    result.Errors.Add("DefaultConnection string format appears to be invalid.");
                }
            }

            // Check for read-only connection string (optional)
            var readOnlyConnection = connectionStrings["ReadOnlyConnection"];
            if (!string.IsNullOrWhiteSpace(readOnlyConnection) && !IsValidConnectionString(readOnlyConnection))
            {
                result.Warnings.Add("ReadOnlyConnection string format appears to be invalid.");
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error validating connection strings: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Validates required environment variables.
    /// </summary>
    /// <returns>Validation result for environment variables.</returns>
    private async Task<ConfigurationValidationResult> ValidateRequiredEnvironmentVariablesAsync()
    {
        var result = new ConfigurationValidationResult();

        try
        {
            var requiredEnvVars = new[]
            {
                "ASPNETCORE_ENVIRONMENT",
                "ADMSServerFilesPath"
            };

            foreach (var envVar in requiredEnvVars)
            {
                var value = Environment.GetEnvironmentVariable(envVar);
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (envVar == "ADMSServerFilesPath")
                    {
                        result.Warnings.Add($"Environment variable '{envVar}' is not set. Using default path.");
                    }
                    else
                    {
                        result.Errors.Add($"Required environment variable '{envVar}' is not set.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error validating environment variables: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Performs additional custom validations for specific configuration types.
    /// </summary>
    /// <typeparam name="T">The configuration type.</typeparam>
    /// <param name="sectionName">The section name.</param>
    /// <param name="options">The configuration options.</param>
    /// <param name="result">The validation result to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task PerformCustomValidationsAsync<T>(string sectionName, T options, ConfigurationValidationResult result) where T : class
    {
        try
        {
            switch (options)
            {
                case FileStorageSettings fileSettings:
                    await ValidateFileStorageAccessibilityAsync(fileSettings, result);
                    break;

                case VirusScannerSettings virusSettings:
                    await ValidateVirusScannerConnectivityAsync(virusSettings, result);
                    break;

                case EmailSettings emailSettings:
                    await ValidateEmailSettingsAsync(emailSettings, result);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error performing custom validation for section: {SectionName}", sectionName);
            result.Warnings.Add($"Could not perform advanced validation for {sectionName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates file storage accessibility.
    /// </summary>
    /// <param name="settings">The file storage settings.</param>
    /// <param name="result">The validation result to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ValidateFileStorageAccessibilityAsync(FileStorageSettings settings, ConfigurationValidationResult result)
    {
        try
        {
            if (!Directory.Exists(settings.BasePath))
            {
                result.Warnings.Add($"File storage base path does not exist: {settings.BasePath}");
            }
            else
            {
                // Test write permissions
                var testFile = Path.Combine(settings.BasePath, $"test_{Guid.NewGuid()}.tmp");
                try
                {
                    await File.WriteAllTextAsync(testFile, "test");
                    File.Delete(testFile);
                }
                catch
                {
                    result.Warnings.Add($"No write permissions to file storage path: {settings.BasePath}");
                }
            }
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Could not validate file storage accessibility: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates virus scanner connectivity.
    /// </summary>
    /// <param name="settings">The virus scanner settings.</param>
    /// <param name="result">The validation result to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ValidateVirusScannerConnectivityAsync(VirusScannerSettings settings, ConfigurationValidationResult result)
    {
        if (!settings.Enabled)
        {
            result.Warnings.Add("Virus scanner is disabled");
            return;
        }

        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            var connectTask = client.ConnectAsync(settings.Host, settings.Port);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

            if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
            {
                result.Warnings.Add($"Cannot connect to virus scanner at {settings.Host}:{settings.Port} (timeout)");
            }
            else if (!client.Connected)
            {
                result.Warnings.Add($"Cannot connect to virus scanner at {settings.Host}:{settings.Port}");
            }
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Virus scanner connectivity check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates email settings.
    /// </summary>
    /// <param name="settings">The email settings.</param>
    /// <param name="result">The validation result to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ValidateEmailSettingsAsync(EmailSettings settings, ConfigurationValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(settings.SmtpHost))
        {
            result.Warnings.Add("SMTP host is not configured");
            return;
        }

        if (!IsValidEmailAddress(settings.SenderEmail))
        {
            result.Errors.Add($"Invalid sender email address: {settings.SenderEmail}");
        }

        // Basic SMTP connectivity test could be added here
        await Task.CompletedTask;
    }

    /// <summary>
    /// Checks environment-specific configuration settings.
    /// </summary>
    /// <param name="health">The configuration health object to update.</param>
    private void CheckEnvironmentSpecificSettings(ConfigurationHealth health)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var envSection = new ConfigurationSectionHealth
        {
            SectionName = "Environment",
            Exists = true,
            HasValues = true,
            Status = ConfigurationSectionStatus.Healthy
        };

        // Check for development-specific warnings
        if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
        {
            var detailedErrors = _configuration.GetValue<bool>("ApiSettings:EnableDetailedErrors", false);
            if (detailedErrors)
            {
                envSection.Issues.Add("Detailed errors are enabled (development only)");
            }
        }

        // Check for production-specific requirements
        if (string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase))
        {
            var requireHttps = _configuration.GetValue<bool>("Security:RequireHttps", true);
            if (!requireHttps)
            {
                envSection.Status = ConfigurationSectionStatus.Warning;
                envSection.Issues.Add("HTTPS requirement is disabled in production");
            }
        }

        health.Sections.Add(envSection);
    }

    /// <summary>
    /// Validates if a connection string appears to be in a valid format.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <returns>True if the format appears valid, false otherwise.</returns>
    private static bool IsValidConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        // Basic validation - must contain either Server/Data Source and Database/Initial Catalog
        var lower = connectionString.ToLowerInvariant();
        var hasServer = lower.Contains("server=") || lower.Contains("data source=");
        var hasDatabase = lower.Contains("database=") || lower.Contains("initial catalog=");

        return hasServer && hasDatabase;
    }

    /// <summary>
    /// Validates if an email address is in a valid format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the format is valid, false otherwise.</returns>
    private static bool IsValidEmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}

/// <summary>
/// Represents the result of configuration validation.
/// </summary>
public class ConfigurationValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the configuration is valid.
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public List<string> Errors { get; } = new();

    /// <summary>
    /// Gets the validation warnings.
    /// </summary>
    public List<string> Warnings { get; } = new();
}

/// <summary>
/// Represents configuration health information.
/// </summary>
public class ConfigurationHealth
{
    /// <summary>
    /// Gets or sets the overall configuration status.
    /// </summary>
    public ConfigurationStatus OverallStatus { get; set; }

    /// <summary>
    /// Gets or sets when the health check was performed.
    /// </summary>
    public DateTime CheckedAt { get; set; }

    /// <summary>
    /// Gets the configuration section health information.
    /// </summary>
    public List<ConfigurationSectionHealth> Sections { get; } = new();
}

/// <summary>
/// Represents health information for a specific configuration section.
/// </summary>
public class ConfigurationSectionHealth
{
    /// <summary>
    /// Gets or sets the section name.
    /// </summary>
    public string SectionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the section exists.
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the section has values.
    /// </summary>
    public bool HasValues { get; set; }

    /// <summary>
    /// Gets or sets the section status.
    /// </summary>
    public ConfigurationSectionStatus Status { get; set; }

    /// <summary>
    /// Gets any issues found with this section.
    /// </summary>
    public List<string> Issues { get; } = new();
}

/// <summary>
/// Enumeration of configuration status values.
/// </summary>
public enum ConfigurationStatus
{
    /// <summary>Configuration is healthy.</summary>
    Healthy,
    /// <summary>Configuration has warnings but is functional.</summary>
    Warning,
    /// <summary>Configuration has errors and may not be functional.</summary>
    Unhealthy
}

/// <summary>
/// Enumeration of configuration section status values.
/// </summary>
public enum ConfigurationSectionStatus
{
    /// <summary>Section is healthy.</summary>
    Healthy,
    /// <summary>Section has warnings.</summary>
    Warning,
    /// <summary>Section has errors.</summary>
    Error
}

/// <summary>
/// Extension methods for configuration validation.
/// </summary>
public static class ConfigurationValidationExtensions
{
    /// <summary>
    /// Adds configuration validation services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConfigurationValidation(this IServiceCollection services)
    {
        services.AddSingleton<IConfigurationValidationService, ConfigurationValidationService>();

        // Add validators for each configuration section
        services.AddSingleton<IValidateOptions<ApiSettings>, ApiSettingsValidator>();
        services.AddSingleton<IValidateOptions<FileStorageSettings>, FileStorageSettingsValidator>();
        services.AddSingleton<IValidateOptions<SecuritySettings>, SecuritySettingsValidator>();
        services.AddSingleton<IValidateOptions<PaginationSettings>, PaginationSettingsValidator>();

        return services;
    }

    /// <summary>
    /// Validates configuration on application startup.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication ValidateConfigurationOnStartup(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var validationService = scope.ServiceProvider.GetRequiredService<IConfigurationValidationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            var result = validationService.ValidateAllAsync().GetAwaiter().GetResult();

            if (!result.IsValid)
            {
                logger.LogError("Configuration validation failed:");
                foreach (var error in result.Errors)
                {
                    logger.LogError("  - {Error}", error);
                }

                throw new InvalidOperationException("Application configuration is invalid. See logs for details.");
            }

            if (result.Warnings.Count > 0)
            {
                logger.LogWarning("Configuration validation completed with warnings:");
                foreach (var warning in result.Warnings)
                {
                    logger.LogWarning("  - {Warning}", warning);
                }
            }
            else
            {
                logger.LogInformation("Configuration validation passed successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate configuration on startup");
            throw;
        }

        return app;
    }
}