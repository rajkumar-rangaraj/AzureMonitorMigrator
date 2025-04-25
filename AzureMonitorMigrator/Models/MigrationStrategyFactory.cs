using System;
using System.Collections.Generic;
using System.Linq;

namespace MCPProject.Models
{
    /// <summary>
    /// Factory for creating and managing migration strategies
    /// </summary>
    public class MigrationStrategyFactory
    {
        private readonly List<IMigrationStrategy> _strategies = new List<IMigrationStrategy>();

        public MigrationStrategyFactory()
        {
            // Register default strategies - these will be implemented later
        }

        /// <summary>
        /// Registers a migration strategy
        /// </summary>
        public void RegisterStrategy(IMigrationStrategy strategy)
        {
            _strategies.Add(strategy);
        }

        /// <summary>
        /// Gets all registered strategies
        /// </summary>
        public IEnumerable<IMigrationStrategy> GetAllStrategies()
        {
            return _strategies;
        }

        /// <summary>
        /// Gets all app type names from registered strategies
        /// </summary>
        public IEnumerable<string> GetAppTypeNames()
        {
            return _strategies.Select(s => s.AppTypeName).Distinct();
        }

        /// <summary>
        /// Gets a strategy that can handle the given project context
        /// </summary>
        public IMigrationStrategy GetStrategy(ProjectAnalysisContext context)
        {
            return _strategies.FirstOrDefault(s => s.CanHandle(context)) 
                ?? throw new InvalidOperationException("No suitable migration strategy found for the project");
        }

        /// <summary>
        /// Gets a strategy by app type name
        /// </summary>
        public IMigrationStrategy GetStrategyByAppType(string appType)
        {
            return _strategies.FirstOrDefault(s => s.AppTypeName.Equals(appType, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Migration strategy for app type '{appType}' not found");
        }
    }
}