using System.Collections.Generic;

namespace MCPProject.Models
{
    /// <summary>
    /// Represents the result of a migration analysis
    /// </summary>
    public class MigrationResult
    {
        public List<string> Findings { get; set; } = new List<string>();
        public List<string> Suggestions { get; set; } = new List<string>();
        public string MigrationSteps { get; set; } = string.Empty;
        public string SampleCode { get; set; } = string.Empty;
        public bool HasAppInsightsReferences => Findings.Count > 0;
        
        public override string ToString()
        {
            if (Findings.Count == 0)
            {
                return "No Application Insights SDK usage detected in the provided project path.";
            }

            var result = new System.Text.StringBuilder();
            result.AppendLine("## Application Insights SDK Detection Results");
            result.AppendLine("");

            result.AppendLine("### Findings:");
            foreach (var finding in Findings)
            {
                result.AppendLine($"- {finding}");
            }

            result.AppendLine("");
            result.AppendLine("### Migration Suggestions:");
            foreach (var suggestion in Suggestions)
            {
                result.AppendLine($"- {suggestion}");
            }

            result.AppendLine("");
            result.AppendLine("### Migration Steps:");
            result.AppendLine(MigrationSteps);

            if (!string.IsNullOrEmpty(SampleCode))
            {
                result.AppendLine("");
                result.AppendLine("### Sample Code:");
                result.AppendLine(SampleCode);
            }

            return result.ToString();
        }
    }
}