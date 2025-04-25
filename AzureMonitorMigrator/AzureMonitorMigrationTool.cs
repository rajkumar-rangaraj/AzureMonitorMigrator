using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.RegularExpressions;
using MCPProject.Models;
using MCPProject.Models.Strategies;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCPProject
{
    [McpServerToolType]
    public static class AzureMonitorMigrationTool
    {
        // Initialize the strategy factory with all available strategies - both hardcoded and from rules
        private static readonly MigrationStrategyFactory _strategyFactory = InitializeStrategyFactory();

        private static MigrationStrategyFactory InitializeStrategyFactory()
        {
            var factory = new MigrationStrategyFactory();

            // Load strategies from JSON rule files
            var ruleLoader = new MigrationRuleLoader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Rules"));
            var rules = ruleLoader.LoadAllRules();
            
            foreach (var rule in rules)
            {
                factory.RegisterStrategy(new ConfigurableMigrationStrategy(rule));
            }
            
            // Register hardcoded strategies as fallback
            factory.RegisterStrategy(new AspNetCoreMigrationStrategy());
            factory.RegisterStrategy(new ConsoleMigrationStrategy());
            factory.RegisterStrategy(new WorkerServiceMigrationStrategy());
            
            return factory;
        }

        [McpServerTool, Description("Analyzes a C# project and suggests changes needed to migrate from Application Insights SDK to Azure Monitor OpenTelemetry Distro.")]
        public static string AnalyzeProject(string projectPath)
        {
            if (!Directory.Exists(projectPath))
            {
                return $"Error: Directory {projectPath} does not exist.";
            }

            // Create the project analysis context
            var context = new ProjectAnalysisContext
            {
                ProjectPath = projectPath,
                CsProjectFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories).ToList(),
                CSharpFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories).ToList()
            };

            // Find the best matching strategy for the project type
            IMigrationStrategy strategy;
            try
            {
                strategy = _strategyFactory.GetStrategy(context);
            }
            catch (InvalidOperationException)
            {
                // Default to ASP.NET Core strategy if no specific match
                strategy = _strategyFactory.GetStrategyByAppType("ASP.NET Core");
            }

            // Generate migration guidance using the selected strategy
            var result = strategy.GenerateMigration(context);
            
            return result.ToString();
        }

        [McpServerTool, Description("Generates sample migration code for replacing Application Insights SDK with Azure Monitor OpenTelemetry Distro.")]
        public static string GenerateMigrationCode(string appType)
        {
            try
            {
                // Get the strategy for the specified app type
                var strategy = _strategyFactory.GetStrategyByAppType(appType);
                return strategy.GenerateSampleCode();
            }
            catch (InvalidOperationException)
            {
                // Return list of supported app types if not found
                var supportedTypes = string.Join(", ", _strategyFactory.GetAppTypeNames());
                return $"Unsupported app type: '{appType}'. Supported types are: {supportedTypes}";
            }
        }

        [McpServerTool, Description("Checks if a project file or folder contains Application Insights SDK references that need migration.")]
        public static string CheckForApplicationInsights(string filePath)
        {
            if (!File.Exists(filePath) && !Directory.Exists(filePath))
            {
                return $"Error: The file or directory {filePath} does not exist.";
            }

            if (File.Exists(filePath))
            {
                // Check a single file
                return CheckSingleFile(filePath);
            }
            else
            {
                // Check a directory
                return CheckDirectory(filePath);
            }
        }

        [McpServerTool, Description("Lists all supported application types for migration code generation.")]
        public static string ListSupportedAppTypes()
        {
            var appTypes = _strategyFactory.GetAppTypeNames().ToList();
            return $"Supported application types for migration: {string.Join(", ", appTypes)}";
        }

        [McpServerTool, Description("Creates a new rule file for a custom migration scenario.")]
        public static string CreateMigrationRule(string appType, string ruleFilePath)
        {
            // Create a basic rule template that the user can customize
            var rule = new MigrationRule
            {
                AppType = appType,
                DetectionPatterns = new List<DetectionPattern>
                {
                    new DetectionPattern
                    {
                        FileType = "csproj",
                        Pattern = "YourDetectionPattern",
                        IsRegex = false
                    }
                },
                AppInsightsIndicators = new List<string>
                {
                    "Microsoft.ApplicationInsights",
                    "TelemetryClient"
                },
                MigrationSuggestions = new List<string>
                {
                    $"Custom migration suggestion for {appType}"
                },
                MigrationSteps = new List<string>
                {
                    $"1. Migration step 1 for {appType}",
                    $"2. Migration step 2 for {appType}"
                },
                SampleCode = $"```csharp\n// Sample migration code for {appType}\n```"
            };

            try
            {
                // Save the rule to the specified path or generate a filename
                string saveFilePath = ruleFilePath;
                if (string.IsNullOrEmpty(saveFilePath) || !saveFilePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    // Create a safe filename from the app type
                    var safeFileName = Regex.Replace(appType.ToLowerInvariant(), @"[^\w\-]", "_") + ".json";
                    saveFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Rules", safeFileName);
                }

                var directory = Path.GetDirectoryName(saveFilePath);
                if (!Directory.Exists(directory) && directory != null)
                {
                    Directory.CreateDirectory(directory);
                }

                var ruleLoader = new MigrationRuleLoader();
                ruleLoader.SaveRule(rule, Path.GetFileName(saveFilePath));

                return $"Migration rule template created for '{appType}' at {saveFilePath}. " +
                       "Customize it with your specific detection patterns and migration steps.";
            }
            catch (Exception ex)
            {
                return $"Error creating migration rule: {ex.Message}";
            }
        }

        private static string CheckSingleFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            var content = File.ReadAllText(filePath);

            switch (extension)
            {
                case ".csproj":
                case ".properties":
                case ".props":
                case ".targets":
                    if (content.Contains("Microsoft.ApplicationInsights") ||
                        content.Contains("ApplicationInsights.AspNetCore"))
                    {
                        return "Application Insights SDK detected in the project file. Migration to Azure Monitor OpenTelemetry Distro is recommended.";
                    }
                    break;

                case ".cs":
                    if (content.Contains("Microsoft.ApplicationInsights") ||
                        content.Contains("TelemetryClient") ||
                        content.Contains("ApplicationInsightsServiceOptions") ||
                        content.Contains("AddApplicationInsightsTelemetry") ||
                        Regex.IsMatch(content, @"\.Track(Event|Exception|Request|Dependency|Metric|Trace|PageView)\("))
                    {
                        return "Application Insights SDK usage detected in the C# file. Migration to Azure Monitor OpenTelemetry Distro is recommended.";
                    }
                    break;
            }

            return $"No Application Insights SDK usage detected in the {extension} file.";
        }

        private static string CheckDirectory(string dirPath)
        {
            bool found = false;
            var fileFindings = new List<string>();

            // Check .csproj files
            foreach (var file in Directory.GetFiles(dirPath, "*.csproj", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(file);
                if (content.Contains("Microsoft.ApplicationInsights") ||
                    content.Contains("ApplicationInsights.AspNetCore"))
                {
                    found = true;
                    fileFindings.Add($"Project file: {Path.GetFileName(file)}");
                }
            }

            // Check .cs files
            foreach (var file in Directory.GetFiles(dirPath, "*.cs", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(file);
                if (content.Contains("Microsoft.ApplicationInsights") ||
                    content.Contains("TelemetryClient") ||
                    content.Contains("ApplicationInsightsServiceOptions") ||
                    content.Contains("AddApplicationInsightsTelemetry") ||
                    Regex.IsMatch(content, @"\.Track(Event|Exception|Request|Dependency|Metric|Trace|PageView)\("))
                {
                    found = true;
                    fileFindings.Add($"C# file: {Path.GetFileName(file)}");
                    // Limit the number of findings to avoid excessive output
                    if (fileFindings.Count >= 10)
                    {
                        fileFindings.Add("... and more (limited to first 10 findings)");
                        break;
                    }
                }
            }

            // Check properties files if not found yet
            if (!found || fileFindings.Count < 10)
            {
                string[] propPatterns = { "*.properties", "*.props", "*.targets" };
                foreach (var pattern in propPatterns)
                {
                    foreach (var file in Directory.GetFiles(dirPath, pattern, SearchOption.AllDirectories))
                    {
                        var content = File.ReadAllText(file);
                        if (content.Contains("ApplicationInsights") ||
                            content.Contains("InstrumentationKey") ||
                            content.Contains("APPINSIGHTS_"))
                        {
                            found = true;
                            fileFindings.Add($"Properties file: {Path.GetFileName(file)}");
                            // Limit the number of findings
                            if (fileFindings.Count >= 10)
                            {
                                fileFindings.Add("... and more (limited to first 10 findings)");
                                break;
                            }
                        }
                    }
                    if (fileFindings.Count >= 10) break;
                }
            }

            if (found)
            {
                var result = "Application Insights SDK usage detected in the directory.\n";
                result += "Files with Application Insights references:\n- " + string.Join("\n- ", fileFindings);
                result += "\n\nRun the AnalyzeProject tool for detailed findings and migration suggestions.";
                return result;
            }

            return "No Application Insights SDK usage detected in the directory.";
        }
    }
}