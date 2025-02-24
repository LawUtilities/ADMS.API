using ADMS.API.DbContexts;
using ADMS.API.Services;

using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json.Serialization;

using Serilog;

using System.Reflection;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NMaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWX1feHZVQ2lcV012WEc=");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/ADMS.API.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
    options.CacheProfiles.Add("240SecondsCacheProfile",
                new() { Duration = 240 });
})
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    })
    .AddXmlDataContractSerializerFormatters()
    .ConfigureApiBehaviorOptions(setupAction =>
    {
        setupAction.InvalidModelStateResponseFactory = context =>
        {
            // create a validation problem details object
            var problemDetailsFactory = context.HttpContext.RequestServices
                .GetRequiredService<ProblemDetailsFactory>();

            var validationProblemDetails = problemDetailsFactory
                .CreateValidationProblemDetails(
                    context.HttpContext,
                    context.ModelState);

            // add additional info not added by default
            validationProblemDetails.Detail =
                "See the errors field for details.";
            validationProblemDetails.Instance =
                context.HttpContext.Request.Path;

            // report invalid model state responses as validation issues
            validationProblemDetails.Type =
                "https://courselibrary.com/modelvalidationproblem";
            validationProblemDetails.Status =
                StatusCodes.Status422UnprocessableEntity;
            validationProblemDetails.Title =
                "One or more validation errors occurred.";

            return new UnprocessableEntityObjectResult(
                validationProblemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(setupAction =>
{
    string xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    setupAction.IncludeXmlComments(xmlCommentsFullPath);
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContextFactory<AdmsContext>(
    dbContextOptions => dbContextOptions.UseSqlServer(
        builder
        .Configuration["ConnectionStrings:ADMS.API.Connection"])
        .EnableSensitiveDataLogging()
    
    );
}
else
{
    builder.Services.AddDbContextFactory<AdmsContext>(
    dbContextOptions => dbContextOptions.UseSqlServer(
        builder
        .Configuration["ConnectionStrings:ADMS.API.Connection"])
    );
}

builder.Services.AddTransient<IPropertyMappingService,
    PropertyMappingService>();

builder.Services.AddTransient<IPropertyCheckerService,
    PropertyCheckerService>();

builder.Services.AddScoped<IAdmsRepository, AdmsRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddResponseCaching();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.Configure<MvcOptions>(config =>
{
    var newtonsoftJsonOutputFormatter = config.OutputFormatters
          .OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

    newtonsoftJsonOutputFormatter?.SupportedMediaTypes
        .Add("application/vnd.adms.hateoas+json");
});

builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new ApiVersion(1, 0);
    setupAction.ReportApiVersions = true;
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An unexpected fault happened.  Try again later.");
        });
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
