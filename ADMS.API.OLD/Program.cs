using ADMS.API.DbContexts;
using ADMS.API.Services;
using ADMS.API.Services.Common;

using Asp.Versioning;

using Mapster;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Serilog;

using Syncfusion.Licensing;

using System.Reflection;

// 1. Read Syncfusion license from environment variable or configuration (do NOT hardcode in source)
var builder = WebApplication.CreateBuilder(args);

var syncfusionLicense = builder.Configuration["SyncfusionLicenseKey"];
if (!string.IsNullOrWhiteSpace(syncfusionLicense))
    SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);

// 2. Serilog configuration from appsettings.json, with context enrichment
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// 3. Add services to the container.
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
    options.CacheProfiles.Add("240SecondsCacheProfile",
        new CacheProfile { Duration = 240 });
})
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    })
    .AddXmlDataContractSerializerFormatters()
    .ConfigureApiBehaviorOptions(setupAction =>
    {
        setupAction.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => new
                {
                    Field = e.Key,
                    Errors = e.Value?.Errors.Select(x => x.ErrorMessage)
                });

            return new BadRequestObjectResult(new { Errors = errors });
        };
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(setupAction =>
{
    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    setupAction.IncludeXmlComments(xmlCommentsFullPath);
});

builder.Services.AddDbContextFactory<AdmsContext>(dbContextOptions =>
    dbContextOptions.UseSqlServer(
        builder.Configuration.GetConnectionString("ADMS.API.Connection"))
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

builder.Services.Configure<EntityValidationOptions>(builder.Configuration.GetSection("EntityValidation"));
builder.Services.Configure<BatchValidationOptions>(builder.Configuration.GetSection("BatchValidation"));

builder.Services.AddSingleton<IPropertyMappingService, PropertyMappingService>();
builder.Services.AddSingleton<IPropertyCheckerService, PropertyCheckerService>();
builder.Services.AddScoped<IEntityExistenceValidator, EntityExistenceValidator>();
builder.Services.AddScoped<IBatchEntityValidator, BatchEntityValidator>();
builder.Services.AddScoped<IEntityExistenceValidator, EntityExistenceValidator>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IAdmsRepository, AdmsRepository>();
builder.Services.AddScoped<IVirusScanner, ClamAvVirusScanner>();
builder.Services.AddScoped<IFileStorage, FileSystemStorage>();

builder.Services.AddMapster();
builder.Services.AddResponseCaching();

builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new ApiVersion(1, 0);
    setupAction.ReportApiVersions = true;
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

// 5. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Use ProblemDetails for error responses in production
    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";
            var problem = new
            {
                status = 500,
                title = "An unexpected fault happened.",
                detail = "Try again later."
            };
            await context.Response.WriteAsync(JsonConvert.SerializeObject(problem));
        });
    });
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exceptionHandlerPathFeature?.Error, "Unhandled exception");

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/problem+json";
        var problem = new
        {
            status = 500,
            title = "An unexpected error occurred.",
            detail = "Please try again later."
        };
        await context.Response.WriteAsync(JsonConvert.SerializeObject(problem));
    });
});

// 6. Add security headers (expanded)
AddSecurityHeaders(app);

app.UseHttpsRedirection();
app.UseRouting();
app.UseResponseCaching(); // Place before Authorization for best effect
app.UseAuthorization();

// Minimal API: Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("System");

// Minimal API: Version info endpoint
app.MapGet("/version", () =>
    {
        var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
        return Results.Ok(new { version });
    })
    .WithName("VersionInfo")
    .WithTags("System");

// Replace the UseEndpoints call with top-level route registrations
app.MapControllers();

await app.RunAsync();
return;


// 4. Security headers middleware
static void AddSecurityHeaders(IApplicationBuilder application)
{
    application.Use(async (context, next) =>
    {
        var headers = context.Response.Headers;

        // Prevent MIME type sniffing
        headers.TryAdd("X-Content-Type-Options", "nosniff");

        // Prevent clickjacking
        headers.TryAdd("X-Frame-Options", "DENY");

        // Content Security Policy (adjust as needed)
        headers.TryAdd("Content-Security-Policy", "default-src 'self'");

        // Referrer Policy
        headers.TryAdd("Referrer-Policy", "no-referrer");

        // HSTS (only for HTTPS)
        if (context.Request.IsHttps)
            headers.TryAdd("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

        // Permissions Policy (restricts browser features)
        headers.TryAdd("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        // Cross-Origin headers
        headers.TryAdd("Cross-Origin-Opener-Policy", "same-origin");
        headers.TryAdd("Cross-Origin-Resource-Policy", "same-origin");

        await next();
    });
}