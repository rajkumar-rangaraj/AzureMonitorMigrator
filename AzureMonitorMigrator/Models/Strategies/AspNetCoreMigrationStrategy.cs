using System.IO;
using System.Linq;

namespace MCPProject.Models.Strategies
{
    public class AspNetCoreMigrationStrategy : BaseMigrationStrategy
    {
        public override string AppTypeName => "ASP.NET Core";

        public override bool CanHandle(ProjectAnalysisContext context)
        {
            // Check for ASP.NET Core project markers
            return context.CsProjectFiles.Any(file => 
            {
                var content = File.ReadAllText(file);
                return content.Contains("Microsoft.AspNetCore.App") || 
                       content.Contains("Microsoft.AspNetCore.Mvc") ||
                       content.Contains("<TargetFramework>net");
            });
        }

        public override string GenerateSampleCode()
        {
            return "```csharp\n" +
                   "// Program.cs\n" +
                   "using Azure.Monitor.OpenTelemetry.AspNetCore;\n\n" +
                   "var builder = WebApplication.CreateBuilder(args);\n\n" +
                   "// Add Azure Monitor OpenTelemetry\n" +
                   "builder.Services.AddOpenTelemetry().UseAzureMonitor(options => {\n" +
                   "    // Connection string can be specified in code, appsettings.json, or environment variables\n" +
                   "    options.ConnectionString = \"InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://regionname.in.applicationinsights.azure.com/\";\n" +
                   "});\n\n" +
                   "// Sample for custom operations tracking\n" +
                   "using System.Diagnostics;\n\n" +
                   "public class MyService {\n" +
                   "    private readonly ActivitySource _activitySource = new ActivitySource(\"MyCompany.MyApp\");\n\n" +
                   "    public void DoSomething() {\n" +
                   "        // Start a new activity (replaces TelemetryClient.StartOperation)\n" +
                   "        using var activity = _activitySource.StartActivity(\"CustomOperation\");\n" +
                   "        activity?.SetTag(\"customProperty\", \"value\");\n" +
                   "        \n" +
                   "        // Your code here\n" +
                   "        \n" +
                   "        // Activity stops automatically when disposed\n" +
                   "    }\n" +
                   "}\n```";
        }

        public override MigrationResult GenerateMigration(ProjectAnalysisContext context)
        {
            var result = base.GenerateMigration(context);
            
            // Add ASP.NET Core specific suggestions
            result.Suggestions.Add("Replace AddApplicationInsightsTelemetry() with AddOpenTelemetry().UseAzureMonitor()");
            result.Suggestions.Add("Use Azure.Monitor.OpenTelemetry.AspNetCore package");
            
            // Add sample code
            result.SampleCode = GenerateSampleCode();
            
            return result;
        }
    }
}