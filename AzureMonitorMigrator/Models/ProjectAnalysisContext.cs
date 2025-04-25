using System.Collections.Generic;

namespace MCPProject.Models
{
    /// <summary>
    /// Represents the context for analyzing a project for migration
    /// </summary>
    public class ProjectAnalysisContext
    {
        public string ProjectPath { get; set; } = string.Empty;
        public List<string> CsProjectFiles { get; set; } = new List<string>();
        public List<string> CSharpFiles { get; set; } = new List<string>();
        public bool HasAppInsightsReferences { get; set; }
        public string ProjectType { get; set; } = "Unknown";
        public Dictionary<string, List<string>> FileFindings { get; set; } = new Dictionary<string, List<string>>();
    }
}