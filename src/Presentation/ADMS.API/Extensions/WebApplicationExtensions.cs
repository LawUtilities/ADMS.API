using ADMS.API.Middleware;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using Serilog;

using System.Text.Json;

namespace ADMS.API.Extensions;

/// <summary>
/// Extension methods for configuring the web application request pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the complete HTTP request pipeline for the ADMS API.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <returns>The configured web application.</returns>
    public static WebApplication ConfigureRequestPipeline(this WebApplication app)
    {
        // Configure development-specific middleware
        if (app.Environment.IsDevelopment())
        {
            app.ConfigureDevelopmentPipeline();
        }
        else
        {
            app.ConfigureProductionPipeline();
        }

        // Configure common middleware (order is important)
        app.ConfigureCommonPipeline();

        return app;
    }

    /// <summary>
    /// Configures middleware specific to development environment.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    private static void ConfigureDevelopmentPipeline(this WebApplication app)
    {
        // Developer exception page for detailed error information
        app.UseDeveloperExceptionPage();

        // Swagger UI for API documentation and testing
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ADMS API v1");
            options.RoutePrefix = string.Empty; // Serve Swagger UI at root
            options.EnableTryItOutByDefault();
            options.DisplayRequestDuration();
            options.EnableFilter();
            options.ShowExtensions();
        });

        // CORS - more permissive in development
        app.UseCors("DefaultPolicy");
    }

    /// <summary>
    /// Configures middleware specific to production environment.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    private static void ConfigureProductionPipeline(this WebApplication app)
    {
        // Global exception handler for production
        app.UseExceptionHandler("/error");

        // HSTS for security
        app.UseHsts();

        // Swagger in production (consider removing or securing)
        if (app.Configuration.GetValue<bool>("EnableSwaggerInProduction", false))
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "ADMS API v1");
                options.RoutePrefix = "api-docs";
                options.DocumentTitle = "ADMS API Documentation";
            });
        }
    }

    /// <summary>
    /// Configures middleware common to all environments.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    private static void ConfigureCommonPipeline(this WebApplication app)
    {
        // Request/Response logging
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());

                if (httpContext.User?.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.Identity.Name);
                }

                if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId);
                }
            };
        });

        // Custom middleware pipeline
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<RequestResponseLoggingMiddleware>();

        // Built-in middleware
        app.UseHttpsRedirection();
        app.UseResponseCompression();
        app.UseRouting();
        app.UseRateLimiter();

        // Authentication and Authorization (when implemented)
        // app.UseAuthentication();
        // app.UseAuthorization();

        // Response caching
        app.UseResponseCaching();

        // CORS (if not configured in environment-specific methods)
        if (!app.Environment.IsDevelopment())
        {
            app.UseCors("DefaultPolicy");
        }

        // Health checks
        app.ConfigureHealthChecks();

        // Error handling endpoints
        app.ConfigureErrorHandling();

        // Map controllers
        app.MapControllers();

        // Additional endpoint mappings
        app.MapFallback(async context =>
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Resource not found");
        });
    }

    /// <summary>
    /// Configures health check endpoints.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    private static void ConfigureHealthChecks(this WebApplication app)
    {
        // Basic health check
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    duration = report.TotalDuration,
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        duration = e.Value.Duration,
                        description = e.Value.Description,
                        data = e.Value.Data
                    })
                });
                await context.Response.WriteAsync(result);
            }
        });

        // Ready check (for Kubernetes)
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        // Live check (for Kubernetes)
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });
    }

    /// <summary>
    /// Configures error handling endpoints.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    private static void ConfigureErrorHandling(this WebApplication app)
    {
        // Global error endpoint
        app.Map("/error", (HttpContext context) =>
        {
            var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            var exception = feature?.Error;

            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = "An error occurred while processing your request",
                Detail = app.Environment.IsDevelopment() ? exception?.Message : "Please try again later",
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            if (context.Items.TryGetValue("CorrelationId", out var correlationId))
            {
                problemDetails.Extensions["correlationId"] = correlationId;
            }

            return Results.Problem(problemDetails);
        }).ExcludeFromDescription();
    }
}

/// <summary>
/// Extension methods for adding response headers.
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Adds API version information to the response headers.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="version">The API version.</param>
    public static void AddApiVersion(this HttpResponse response, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        response.Headers.Append("X-API-Version", version);
    }

    /// <summary>
    /// Adds correlation ID to the response headers.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="correlationId">The correlation ID. If null, attempts to get from HttpContext.Items.</param>
    public static void AddCorrelationId(this HttpResponse response, string? correlationId = null)
    {
        correlationId ??= response.HttpContext.Items["CorrelationId"]?.ToString();

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            response.Headers.Append("X-Correlation-ID", correlationId);
        }
    }

    /// <summary>
    /// Adds pagination metadata to the response headers.
    /// </summary>
    /// <typeparam name="T">The type of items in the paged list.</typeparam>
    /// <param name="response">The HTTP response.</param>
    /// <param name="pagedList">The paged list containing pagination information.</param>
    public static void AddPaginationMetadata<T>(this HttpResponse response, PagedList<T> pagedList)
    {
        ArgumentNullException.ThrowIfNull(pagedList);

        var paginationMetadata = new
        {
            totalCount = pagedList.TotalCount,
            pageSize = pagedList.PageSize,
            currentPage = pagedList.CurrentPage,
            totalPages = pagedList.TotalPages,
            hasNext = pagedList.HasNext,
            hasPrevious = pagedList.HasPrevious
        };

        response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
    }
}