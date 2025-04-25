using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MCPProject.Models.Strategies
{
    /// <summary>
    /// A migration strategy that uses rules defined in JSON files
    /// </summary>
    public class ConfigurableMigrationStrategy : BaseMigrationStrategy
    {
        private readonly MigrationRule _rule;

        public ConfigurableMigrationStrategy(MigrationRule rule)
        {
            _rule = rule ?? throw new ArgumentNullException(nameof(rule));
        }

        public override string AppTypeName => _rule.AppType;

        public override bool CanHandle(ProjectAnalysisContext context)
        {
            foreach (var pattern in _rule.DetectionPatterns)
            {
                switch (pattern.FileType.ToLowerInvariant())
                {
                    case "csproj":
                        if (MatchesAnyFile(context.CsProjectFiles, pattern))
                        {
                            return true;
                        }
                        break;
                    case "cs":
                        if (MatchesAnyFile(context.CSharpFiles, pattern))
                        {
                            return true;
                        }
                        break;
                    default:
                        // Handle other file types if needed
                        break;
                }
            }
            
            return false;
        }

        public override string GenerateSampleCode()
        {
            return _rule.SampleCode;
        }

        public override MigrationResult GenerateMigration(ProjectAnalysisContext context)
        {
            var result = new MigrationResult();
            
            // Search for App Insights indicators in files
            foreach (var indicator in _rule.AppInsightsIndicators)
            {
                bool found = false;
                
                // Check project files
                foreach (var file in context.CsProjectFiles)
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains(indicator))
                    {
                        result.Findings.Add($"Found '{indicator}' in {Path.GetFileName(file)}");
                        found = true;
                    }
                }
                
                // Check CS files
                if (!found)
                {
                    foreach (var file in context.CSharpFiles)
                    {
                        var content = File.ReadAllText(file);
                        if (content.Contains(indicator))
                        {
                            result.Findings.Add($"Found '{indicator}' in {Path.GetFileName(file)}");
                            found = true;
                            break;
                        }
                    }
                }
            }
            
            // Add migration suggestions from the rule
            result.Suggestions.AddRange(_rule.MigrationSuggestions);
            
            // Generate migration steps
            result.MigrationSteps = string.Join("\n\n", _rule.MigrationSteps);
            
            // Add sample code
            result.SampleCode = _rule.SampleCode;
            
            return result;
        }

        private bool MatchesAnyFile(System.Collections.Generic.List<string> files, DetectionPattern pattern)
        {
            foreach (var file in files)
            {
                // Check file name filter if specified
                if (!string.IsNullOrEmpty(pattern.FileName) && 
                    !Regex.IsMatch(Path.GetFileName(file), WildcardToRegex(pattern.FileName)))
                {
                    continue;
                }

                var content = File.ReadAllText(file);
                
                // Check pattern
                if (pattern.IsRegex)
                {
                    if (Regex.IsMatch(content, pattern.Pattern))
                    {
                        return true;
                    }
                }
                else
                {
                    if (content.Contains(pattern.Pattern))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";
        }
    }
}