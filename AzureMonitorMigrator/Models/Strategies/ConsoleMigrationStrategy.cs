using System.IO;
using System.Linq;

namespace MCPProject.Models.Strategies
{
    public class ConsoleMigrationStrategy : BaseMigrationStrategy
    {
        public override string AppTypeName => "Console";

        public override bool CanHandle(ProjectAnalysisContext context)
        {
            // Check for Console project markers
            return context.CsProjectFiles.Any(file => 
            {
                var content = File.ReadAllText(file);
                return content.Contains("<OutputType>Exe</OutputType>") &&
                      !content.Contains("Microsoft.AspNetCore") &&
                      !content.Contains("Microsoft.Extensions.Hosting.WindowsServices");
            });
        }

        public override string GenerateSampleCode()
        {
            return "```csharp\n" +
                   "// Program.cs\n" +
                   "using OpenTelemetry;\n" +
                   "using OpenTelemetry.Resources;\n" +
                   "using OpenTelemetry.Trace;\n" +
                   "using Azure.Monitor.OpenTelemetry.Exporter;\n" +
                   "using System.Diagnostics;\n\n" +
                   "// Define your ActivitySource\n" +
                   "var myActivitySource = new ActivitySource(\"MyCompany.MyApp\");\n\n" +
                   "// Configure OpenTelemetry\n" +
                   "using var tracerProvider = Sdk.CreateTracerProviderBuilder()\n" +
                   "    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(\"MyServiceName\"))\n" +
                   "    .AddSource(myActivitySource.Name)\n" +
                   "    .AddAzureMonitorTraceExporter(options => {\n" +
                   "        options.ConnectionString = \"InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://regionname.in.applicationinsights.azure.com/\";\n" +
                   "    })\n" +
                   "    .Build();\n\n" +
                   "// Your application code\n" +
                   "using (var activity = myActivitySource.StartActivity(\"SampleOperation\"))\n" +
                   "{\n" +
                   "    activity?.SetTag(\"customDimension\", \"value\");\n" +
                   "    // Your code here\n" +
                   "}\n```";
        }

        public override MigrationResult GenerateMigration(ProjectAnalysisContext context)
        {
            var result = base.GenerateMigration(context);
            
            // Add Console app specific suggestions
            result.Suggestions.Add("Use the OpenTelemetry SDK with Azure.Monitor.OpenTelemetry.Exporter package");
            result.Suggestions.Add("Initialize OpenTelemetry with TracerProvider in your Program.cs");
            
            // Add sample code
            result.SampleCode = GenerateSampleCode();
            
            return result;
        }
    }
}