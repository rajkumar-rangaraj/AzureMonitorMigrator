using System.IO;
using System.Linq;

namespace MCPProject.Models.Strategies
{
    public class WorkerServiceMigrationStrategy : BaseMigrationStrategy
    {
        public override string AppTypeName => "Worker Service";

        public override bool CanHandle(ProjectAnalysisContext context)
        {
            // Check for Worker Service project markers
            return context.CsProjectFiles.Any(file => 
            {
                var content = File.ReadAllText(file);
                return content.Contains("Microsoft.Extensions.Hosting") && 
                       content.Contains("Microsoft.Extensions.Hosting.WindowsServices");
            }) || context.CSharpFiles.Any(file => 
            {
                var content = File.ReadAllText(file);
                return content.Contains("IHostedService") || 
                      (content.Contains("BackgroundService") && 
                       !content.Contains("Microsoft.AspNetCore"));
            });
        }

        public override string GenerateSampleCode()
        {
            return "```csharp\n" +
                   "// Program.cs\n" +
                   "using Microsoft.Extensions.Hosting;\n" +
                   "using Microsoft.Extensions.DependencyInjection;\n" +
                   "using Azure.Monitor.OpenTelemetry.Exporter;\n" +
                   "using OpenTelemetry;\n" +
                   "using OpenTelemetry.Trace;\n" +
                   "using OpenTelemetry.Resources;\n" +
                   "using System.Diagnostics;\n\n" +
                   "var builder = Host.CreateApplicationBuilder(args);\n\n" +
                   "// Register your worker services\n" +
                   "builder.Services.AddHostedService<Worker>();\n\n" +
                   "// Add Azure Monitor OpenTelemetry\n" +
                   "builder.Services.AddOpenTelemetry()\n" +
                   "    .WithTracing(builder => builder\n" +
                   "        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(\"MyWorkerService\"))\n" +
                   "        .AddSource(\"MyWorkerService\")\n" +
                   "        .AddAzureMonitorTraceExporter(options => {\n" +
                   "            options.ConnectionString = \"InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://regionname.in.applicationinsights.azure.com/\";\n" +
                   "        }));\n\n" +
                   "var host = builder.Build();\n" +
                   "await host.RunAsync();\n\n" +
                   "// Worker.cs\n" +
                   "public class Worker : BackgroundService\n" +
                   "{\n" +
                   "    private readonly ActivitySource _activitySource;\n" +
                   "    private readonly ILogger<Worker> _logger;\n\n" +
                   "    public Worker(ILogger<Worker> logger)\n" +
                   "    {\n" +
                   "        _logger = logger;\n" +
                   "        _activitySource = new ActivitySource(\"MyWorkerService\");\n" +
                   "    }\n\n" +
                   "    protected override async Task ExecuteAsync(CancellationToken stoppingToken)\n" +
                   "    {\n" +
                   "        while (!stoppingToken.IsCancellationRequested)\n" +
                   "        {\n" +
                   "            using (var activity = _activitySource.StartActivity(\"WorkerOperation\"))\n" +
                   "            {\n" +
                   "                activity?.SetTag(\"executionTime\", DateTime.UtcNow);\n" +
                   "                _logger.LogInformation(\"Worker running at: {time}\", DateTimeOffset.Now);\n" +
                   "                \n" +
                   "                try\n" +
                   "                {\n" +
                   "                    // Do work here\n" +
                   "                    await Task.Delay(1000, stoppingToken);\n" +
                   "                }\n" +
                   "                catch (Exception ex)\n" +
                   "                {\n" +
                   "                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);\n" +
                   "                    activity?.RecordException(ex);\n" +
                   "                    _logger.LogError(ex, \"Error executing worker task\");\n" +
                   "                }\n" +
                   "            }\n" +
                   "        }\n" +
                   "    }\n" +
                   "}\n```";
        }

        public override MigrationResult GenerateMigration(ProjectAnalysisContext context)
        {
            var result = base.GenerateMigration(context);
            
            // Add Worker Service specific suggestions
            result.Suggestions.Add("Use Azure.Monitor.OpenTelemetry.Exporter package for Worker Services");
            result.Suggestions.Add("Configure OpenTelemetry in Program.cs with AddOpenTelemetry().WithTracing()");
            result.Suggestions.Add("Use ActivitySource in your BackgroundService implementations");
            
            // Add sample code
            result.SampleCode = GenerateSampleCode();
            
            return result;
        }
    }
}