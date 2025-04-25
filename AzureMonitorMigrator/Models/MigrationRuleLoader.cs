using System.Text.Json;

namespace MCPProject.Models
{
    /// <summary>
    /// Loads migration rules from JSON files
    /// </summary>
    public class MigrationRuleLoader
    {
        private readonly string _rulesDirectory;

        public MigrationRuleLoader(string? rulesDirectory = null)
        {
            _rulesDirectory = rulesDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Rules");
        }

        /// <summary>
        /// Loads all migration rules from JSON files in the rules directory
        /// </summary>
        public List<MigrationRule> LoadAllRules()
        {
            var rules = new List<MigrationRule>();
            
            if (!Directory.Exists(_rulesDirectory))
            {
                return rules;
            }

            var jsonFiles = Directory.GetFiles(_rulesDirectory, "*.json");
            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var rule = JsonSerializer.Deserialize<MigrationRule>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (rule != null)
                    {
                        rules.Add(rule);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading rule from {file}: {ex.Message}");
                }
            }

            return rules;
        }

        /// <summary>
        /// Loads a migration rule from a specific JSON file
        /// </summary>
        public MigrationRule LoadRuleFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Rule file not found: {filePath}");
            }

            var json = File.ReadAllText(filePath);
            var rule = JsonSerializer.Deserialize<MigrationRule>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (rule == null)
            {
                throw new InvalidOperationException($"Failed to parse rule from {filePath}");
            }

            return rule;
        }

        /// <summary>
        /// Saves a migration rule to a JSON file
        /// </summary>
        public void SaveRule(MigrationRule rule, string fileName)
        {
            if (!Directory.Exists(_rulesDirectory))
            {
                Directory.CreateDirectory(_rulesDirectory);
            }

            var filePath = Path.Combine(_rulesDirectory, fileName);
            var json = JsonSerializer.Serialize(rule, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
        }
    }
}