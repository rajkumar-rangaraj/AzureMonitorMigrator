namespace MCPProject.Models
{
    /// <summary>
    /// Interface for migration strategy implementations
    /// </summary>
    public interface IMigrationStrategy
    {
        /// <summary>
        /// Determines if this strategy can handle the given project context
        /// </summary>
        bool CanHandle(ProjectAnalysisContext context);

        /// <summary>
        /// Analyzes the project and generates migration guidance
        /// </summary>
        MigrationResult GenerateMigration(ProjectAnalysisContext context);

        /// <summary>
        /// Generates sample code for the migration
        /// </summary>
        string GenerateSampleCode();

        /// <summary>
        /// The name of the app type this strategy handles
        /// </summary>
        string AppTypeName { get; }
    }
}