using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MCPProject.Models
{
    /// <summary>
    /// Represents a migration rule that can be loaded from a JSON file
    /// </summary>
    public class MigrationRule
    {
        /// <summary>
        /// The application type this rule applies to (e.g., "ASP.NET Core", "Console", "Worker Service")
        /// </summary>
        public string AppType { get; set; } = string.Empty;
        
        /// <summary>
        /// The file patterns to check for when detecting this app type
        /// </summary>
        public List<DetectionPattern> DetectionPatterns { get; set; } = new List<DetectionPattern>();
        
        /// <summary>
        /// List of strings to search for in files to determine if Application Insights is used
        /// </summary>
        public List<string> AppInsightsIndicators { get; set; } = new List<string>();
        
        /// <summary>
        /// Migration suggestions specific to this app type
        /// </summary>
        public List<string> MigrationSuggestions { get; set; } = new List<string>();
        
        /// <summary>
        /// Sample code showing the migration for this app type
        /// </summary>
        public string SampleCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Migration steps specific to this app type
        /// </summary>
        public List<string> MigrationSteps { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Represents a pattern for detecting application types
    /// </summary>
    public class DetectionPattern
    {
        /// <summary>
        /// Type of file to search in (e.g., "csproj", "cs")
        /// </summary>
        public string FileType { get; set; } = string.Empty;
        
        /// <summary>
        /// Pattern to look for in the file
        /// </summary>
        public string Pattern { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the pattern is a regex. If false, it's a simple string contains check.
        /// </summary>
        public bool IsRegex { get; set; }
        
        /// <summary>
        /// Optional file name filter (e.g., "Program.cs", "*.csproj")
        /// </summary>
        public string FileName { get; set; } = string.Empty;
    }
}