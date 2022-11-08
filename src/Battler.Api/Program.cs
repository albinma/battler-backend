using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Events;
using System.Reflection;

Log.Logger = new LoggerConfiguration().MinimumLevel
    .Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web host");

    var builder = WebApplication.CreateBuilder(args);

    // Remove response header "Server: Kestrel" for security reasons
    builder.WebHost.UseKestrel(x => x.AddServerHeader = false);

    // Setup serilog logging provider
    builder.Host.UseSerilog(
        (context, services, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration);
            config.ReadFrom.Services(services);
        }
    );

    builder.Services.AddHealthChecks();

    builder.Services.AddCors();

    builder.Services.AddLocalization();

    builder.Services.AddHttpLogging(
        x =>
            x.LoggingFields = builder.Environment.IsProduction()
                ? HttpLoggingFields.RequestPropertiesAndHeaders
                    | HttpLoggingFields.ResponsePropertiesAndHeaders
                    | HttpLoggingFields.RequestQuery
                : HttpLoggingFields.All
    );

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
    });

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    });

    var app = builder.Build();

    app.UseRequestTracing();

    app.UseHttpLogging();

    app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

    app.UseHealthChecks(Routes.HealthChecks);

    if (!builder.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint(Routes.SwaggerDocument, "Hashit API");
        });
    }

    app.UseRequestLocalization();

    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

    app.Run();

    return 0;
}
catch (Exception ex)
{
    // For silencing EF Core's migration termination error log which is expected.
    // https://stackoverflow.com/a/70256808
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }

    Log.Fatal(ex, "Host terminated unexpectedly");

    return 1;
}
finally
{
    Log.CloseAndFlush();
}
