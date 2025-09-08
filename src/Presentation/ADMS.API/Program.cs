using ADMS.API.Extensions;
using ADMS.API.HealthChecks;
using ADMS.API.Middleware;

using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi.Models;

using Serilog;
using Serilog.Events;

using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace ADMS.API;

/// <summary>
/// The main entry point for the ADMS API application.
/// Configures services, middleware pipeline, and application startup.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main method that starts the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A task representing the application lifetime.</returns>
    public static async Task<int> Main(string[] args)
    {
        // Configure Serilog early to capture startup errors
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting ADMS API application");

            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/adms-api-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30));

            // Add services to the container
            builder.Services.AddApplicationServices(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            app.ConfigureRequestPipeline();

            Log.Information("ADMS API application configured successfully");

            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "ADMS API application terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}

/// <summary>
/// Extension methods for configuring application services.
/// </summary>
public static class ServiceConfigurationExtensions
{
    /// <summary>
    /// Adds all application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure third-party licenses first
        services.ConfigureThirdPartyLicenses(configuration);
        ValidateLicenseConfiguration(configuration);

        // Add core services
        services.AddCoreServices(configuration);

        // Add API-specific services
        services.AddApiServices(configuration);

        // Add infrastructure services
        services.AddInfrastructureServices(configuration);

        // Add shared services
        services.AddSharedServices(configuration);

        return services;
    }

    /// <summary>
    /// Configures third-party software licenses securely.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection ConfigureThirdPartyLicenses(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Syncfusion license
        var syncfusionLicense = configuration["Licenses:SyncfusionLicenseKey"];
        if (!string.IsNullOrEmpty(syncfusionLicense))
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);
            Log.Information("Syncfusion license configured successfully");
        }
        else
        {
            Log.Warning("Syncfusion license key not found in configuration. Some features may not work correctly.");
        }
       
        // Add other license configurations here as needed
        // Example: Configure AutoMapper license if required in future versions

        return services;
    }

    private static void ValidateLicenseConfiguration(IConfiguration configuration)
    {
        var requiredLicenses = new[]
        {
            "Licenses:SyncfusionLicenseKey",
            "Licenses:AutoMapperLicenseKey"
        };

        var missingLicenses = requiredLicenses
            .Where(key => string.IsNullOrEmpty(configuration[key]))
            .ToList();

        if (!missingLicenses.Any()) return;
        var message = $"Missing required license keys: {string.Join(", ", missingLicenses)}";
        Log.Warning(message);

        // Optionally throw in production if licenses are critical
//        if (environment.IsProduction())
//        {
//            throw new InvalidOperationException(message);
//        }
    }

    /// <summary>
    /// Adds API-specific services.
    /// </summary>
    private static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure AutoMapper
        services.AddAutoMapperConfiguration(configuration);

        // Configure controllers with advanced options
        services.AddControllers(options =>
        {
            // Global model validation
            options.ModelValidatorProviders.Clear();

            // Custom model binder providers if needed
            // options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider());

            // Configure cache profiles
            options.CacheProfiles.Add("240SecondsCacheProfile", new CacheProfile
            {
                Duration = 240,
                Location = ResponseCacheLocation.Any
            });

            // Add custom filters
            options.Filters.Add<GlobalExceptionFilter>();
            options.Filters.Add<ModelValidationFilter>();
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.WriteIndented = false;
        })
        .AddXmlSerializerFormatters() // Support XML content negotiation
        .ConfigureApiBehaviorOptions(options =>
        {
            // Custom model validation response
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetailsFactory = context.HttpContext.RequestServices
                    .GetRequiredService<ProblemDetailsFactory>();

                var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                    context.HttpContext, context.ModelState);

                return new BadRequestObjectResult(problemDetails);
            };
        });

        // API Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Version"),
                new QueryStringApiVersionReader("version")
            );
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Problem Details
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = context.HttpContext.Request.Path;
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                if (context.HttpContext.Items.TryGetValue("CorrelationId", out var correlationId))
                {
                    context.ProblemDetails.Extensions["correlationId"] = correlationId;
                }
            };
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                policy.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // Response Compression
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        // Rate Limiting
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });

        // Health Checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<FileStorageHealthCheck>("file-storage");

        // OpenAPI/Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ADMS API",
                Version = "v1",
                Description = "Advanced Document Management System API",
                Contact = new OpenApiContact
                {
                    Name = "ADMS Development Team",
                    Email = "dev@adms.com"
                },
                License = new OpenApiLicense
                {
                    Name = "Proprietary License"
                }
            });

            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Add security definitions if authentication is implemented
            // options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
        });

        // Memory Cache
        services.AddMemoryCache();

        // HttpClient Factory
        services.AddHttpClient();

        return services;
    }

    /// <summary>
    /// Configures AutoMapper with all mapping profiles.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var autoMapperLicense = configuration["Licenses:AutoMapperLicenseKey"];
        if (string.IsNullOrEmpty(autoMapperLicense))
        {
            Log.Warning("AutoMapper license key not found in configuration. Some features may not work correctly.");
            return services;
        }

        // Configure AutoMapper with assembly scanning for profiles
        services.AddAutoMapper(cfg =>
            {
                cfg.LicenseKey = autoMapperLicense;
                // Configure AutoMapper settings
                cfg.AllowNullDestinationValues = true;
                cfg.AllowNullCollections = true;

                // Add custom value resolvers, type converters, etc.
                // configuration.AddProfile<CustomMappingProfile>();

            },
            // Scan these assemblies for AutoMapper profiles
            Assembly.GetExecutingAssembly(),           // Current API assembly
            typeof(ADMS.Application.AssemblyMarker).Assembly,  // Application assembly
            typeof(ADMS.Domain.AssemblyMarker).Assembly        // Domain assembly (if needed)
        );

        // Validate AutoMapper configuration in development
        services.AddSingleton<IHostedService, AutoMapperValidationService>();

        Log.Information("AutoMapper configuration completed successfully");
        return services;
    }
}

/// <summary>
/// Background service to validate AutoMapper configuration on startup.
/// </summary>
public class AutoMapperValidationService : IHostedService
{
    private readonly IMapper _mapper;
    private readonly ILogger<AutoMapperValidationService> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoMapperValidationService"/> class.
    /// </summary>
    /// <param name="mapper">The AutoMapper instance used for object mapping operations.</param>
    /// <param name="logger">The logger instance used for logging diagnostic and error information.</param>
    /// <param name="environment">The host environment providing information about the application's runtime environment.</param>
    public AutoMapperValidationService(
        IMapper mapper,
        ILogger<AutoMapperValidationService> logger,
        IHostEnvironment environment)
    {
        _mapper = mapper;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Performs startup validation for the application in a development environment.
    /// </summary>
    /// <remarks>This method validates the AutoMapper configuration and logs the result. If the configuration
    /// is invalid, an exception is thrown to prevent the application from starting. This validation is only performed
    /// when the application is running in a development environment.</remarks>
    /// <param name="cancellationToken">A token that can be used to cancel the operation. This parameter is not used in the current implementation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task is completed immediately.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment()) return Task.CompletedTask;
        try
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
            _logger.LogInformation("AutoMapper configuration validation passed");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "AutoMapper configuration validation failed");
            throw new InvalidOperationException("AutoMapper configuration validation failed during startup. See inner exception for details.", exception); // Fail fast in development
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the asynchronous operation gracefully.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to signal the request to cancel the stop operation.</param>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous stop operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

// Assembly marker classes for AutoMapper assembly scanning
namespace ADMS.Application
{
    /// <summary>
    /// Marker class for Application assembly identification.
    /// </summary>
    public class AssemblyMarker { }
}

namespace ADMS.Domain
{
    /// <summary>
    /// Marker class for Domain assembly identification.
    /// </summary>
    public class AssemblyMarker { }
}