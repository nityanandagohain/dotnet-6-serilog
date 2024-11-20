using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Serilog;
using OpenTelemetry.Exporter;


var builder = WebApplication.CreateBuilder(args);

// Add at the start of your Program.cs
if (builder.Environment.IsDevelopment())
{
    // Disable SSL certificate validation for development
    HttpClientHandler clientHandler = new HttpClientHandler();
    clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
}

// Add the OTLP package first:
// dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol

// Define OpenTelemetry Resource
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("SerilogOpenTelemetryExample")
    .AddAttributes(new Dictionary<string, object>
    {
        { "environment", "development" },
        { "application", "SerilogExampleApp" }
    });

// Clear default logging providers and add OpenTelemetry
builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;
    options.SetResourceBuilder(resourceBuilder);
    options.AddConsoleExporter(); // This should work now with the new package
});

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Keep console sink for local debugging
    .WriteTo.OpenTelemetry(options =>
    {
        options.Endpoint = "http://localhost:4317"; // OTLP GRPC endpoint
        options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;  // Changed from OtlpProtocol.Grpc to string
        options.ResourceAttributes = resourceBuilder.Build().Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    })
    .CreateLogger();

builder.Host.UseSerilog(); // Use Serilog for ASP.NET Core

// Add services
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
