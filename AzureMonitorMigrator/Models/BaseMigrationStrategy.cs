using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MCPProject.Models
{
    /// <summary>
    /// Base class for migration strategies providing common functionality
    /// </summary>
    public abstract class BaseMigrationStrategy : IMigrationStrategy
    {
        public abstract string AppTypeName { get; }

        public abstract bool CanHandle(ProjectAnalysisContext context);

        public abstract string GenerateSampleCode();

        public virtual MigrationResult GenerateMigration(ProjectAnalysisContext context)
        {
            var result = new MigrationResult();
            
            AnalyzeProjectFiles(context, result);
            AnalyzeCSharpFiles(context, result);
            
            // Deduplicate suggestions
            result.Suggestions = result.Suggestions.Distinct().ToList();
            
            // Generate common migration steps
            result.MigrationSteps = GenerateMigrationSteps();
            
            return result;
        }
        
        protected virtual void AnalyzeProjectFiles(ProjectAnalysisContext context, MigrationResult result)
        {
            foreach (var csprojFile in context.CsProjectFiles)
            {
                var content = File.ReadAllText(csprojFile);
                if (content.Contains("Microsoft.ApplicationInsights") ||
                    content.Contains("ApplicationInsights.AspNetCore"))
                {
                    result.Findings.Add($"Found Application Insights SDK reference in {Path.GetFileName(csprojFile)}");
                    result.Suggestions.Add("Replace all Application Insights SDK packages with latest version of Azure.Monitor.OpenTelemetry.AspNetCore package");
                }
            }
        }
        
        protected virtual void AnalyzeCSharpFiles(ProjectAnalysisContext context, MigrationResult result)
        {
            foreach (var csFile in context.CSharpFiles)
            {
                var content = File.ReadAllText(csFile);

                // Check for TelemetryClient usage
                if (content.Contains("TelemetryClient"))
                {
                    result.Findings.Add($"Found TelemetryClient usage in {Path.GetFileName(csFile)}");
                    result.Suggestions.Add("Replace TelemetryClient with ActivitySource for tracking operations");
                }

                // Check for ApplicationInsights namespace
                if (content.Contains("Microsoft.ApplicationInsights"))
                {
                    result.Findings.Add($"Found Microsoft.ApplicationInsights namespace in {Path.GetFileName(csFile)}");
                }

                // Check for TrackEvent and other tracking methods
                if (Regex.IsMatch(content, @"\.Track(Event|Exception|Request|Dependency|Metric|Trace|PageView)\("))
                {
                    result.Findings.Add($"Found Track methods in {Path.GetFileName(csFile)}");
                    result.Suggestions.Add("Replace Track methods with OpenTelemetry equivalents");
                }

                // Check for Startup.cs or Program.cs configuration
                if (Path.GetFileName(csFile).Equals("Startup.cs", StringComparison.OrdinalIgnoreCase) ||
                    Path.GetFileName(csFile).Equals("Program.cs", StringComparison.OrdinalIgnoreCase))
                {
                    if (content.Contains("AddApplicationInsightsTelemetry"))
                    {
                        result.Findings.Add($"Found AddApplicationInsightsTelemetry in {Path.GetFileName(csFile)}");
                        result.Suggestions.Add("Replace AddApplicationInsightsTelemetry with AddOpenTelemetry().UseAzureMonitor()");
                    }
                }
            }
        }
        
        protected virtual string GenerateMigrationSteps()
        {
            var steps = new System.Text.StringBuilder();
            steps.AppendLine("1. Add the Azure Monitor OpenTelemetry package appropriate for your app type");
            steps.AppendLine("   - For ASP.NET Core: Azure.Monitor.OpenTelemetry.AspNetCore");
            steps.AppendLine("   - For ASP.NET, console, WorkerService: Azure.Monitor.OpenTelemetry.Exporter");
            steps.AppendLine("");
            steps.AppendLine("2. In your Program.cs file, add and configure OpenTelemetry with Azure Monitor:");
            steps.AppendLine("   - Import the Azure.Monitor.OpenTelemetry namespace");
            steps.AppendLine("   - Call services.AddOpenTelemetry().UseAzureMonitor()");
            steps.AppendLine("");
            steps.AppendLine("3. For custom operations tracking, replace TelemetryClient with ActivitySource:");
            steps.AppendLine("   - Create a new ActivitySource");
            steps.AppendLine("   - Use StartActivity() instead of StartOperation()");
            steps.AppendLine("");
            steps.AppendLine("4. Be aware that by March 31, 2025, support for instrumentation key ingestion will end. You should transition to connection strings.");
            
            return steps.ToString();
        }
    }
}