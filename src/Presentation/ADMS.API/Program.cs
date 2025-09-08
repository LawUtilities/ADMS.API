using ADMS.API.Extensions;
using ADMS.API.HealthChecks;
using ADMS.API.Middleware;

using Asp.Versioning;

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
public class Program
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
    /// Adds API-specific services.
    /// </summary>
    private static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZeeHRVQ2VcV0ZzXEZWYEg=");
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
}