using Microsoft.Data.SqlClient;
using System.Text;

namespace ORMTrial2.Tools
{
    public class ModelGenerator
    {
        private readonly SchemaGenerator _schemaGenerator;

        public ModelGenerator()
        {
            _schemaGenerator = new SchemaGenerator();
        }

        public void GenerateModels(string connectionString)
        {
            var solutionDirectory = GetSolutionDirectory();
            if (solutionDirectory == null)
            {
                Console.WriteLine("Solution directory could not be determined.");
                return;
            }

            var modelsDirectory = Path.Combine(solutionDirectory, "Models");
            if (!Directory.Exists(modelsDirectory))
            {
                Console.WriteLine($"The 'Models' folder does not exist. Create it? (y/n)");
                if (!ConfirmAction())
                {
                    Console.WriteLine("Operation cancelled.");
                    return;
                }

                Directory.CreateDirectory(modelsDirectory);
            }

            var tableNames = _schemaGenerator.GetTablesFromDatabase(connectionString);

            foreach (var tableName in tableNames)
            {
                try
                {
                    var columns = _schemaGenerator.GetExistingColumnsFromDatabase(tableName, connectionString);
                    var primaryKey = _schemaGenerator.GetPrimaryKeyForTable(tableName, connectionString);
                    var foreignKeys = _schemaGenerator.GetForeignKeysForTable(tableName, connectionString);
                    var constraints = _schemaGenerator.GetColumnConstraints(tableName, connectionString);

                    var classContent = GenerateClassContent(tableName, columns, primaryKey, foreignKeys, constraints);

                    var filePath = Path.Combine(modelsDirectory, $"{tableName}.cs");
                    if (File.Exists(filePath))
                    {
                        Console.WriteLine($"File '{filePath}' already exists. Overwrite? (y/n)");
                        if (!ConfirmAction())
                        {
                            Console.WriteLine($"Skipped generating model for table '{tableName}'.");
                            continue;
                        }
                    }

                    File.WriteAllText(filePath, classContent);
                    Console.WriteLine($"Model for table '{tableName}' has been generated at '{filePath}'.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error generating model for table '{tableName}': {ex.Message}");
                }
            }
        }

        private string GenerateClassContent(string tableName, Dictionary<string, string> columns, string? primaryKey, Dictionary<string, string>? foreignKeys, Dictionary<string, List<string>>? constraints)
        {
            var classBuilder = new StringBuilder();

            classBuilder.AppendLine("using System;");
            classBuilder.AppendLine("using System.ComponentModel.DataAnnotations;");
            classBuilder.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            classBuilder.AppendLine();
            classBuilder.AppendLine("namespace ORMTrial2.Models");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine($"[Table(\"{tableName}\")]");
            classBuilder.AppendLine($"public class {tableName}");
            classBuilder.AppendLine("{");

            foreach (var column in columns)
            {
                var propertyName = column.Key;
                var sqlType = column.Value;
                var csharpType = GetCSharpType(sqlType);

                // Add [Key] for Primary Key
                if (propertyName.Equals(primaryKey, StringComparison.OrdinalIgnoreCase))
                {
                    classBuilder.AppendLine("[Key]");
                }

                // Add [ForeignKey] for Foreign Keys
                if (foreignKeys != null && foreignKeys.ContainsKey(propertyName))
                {
                    var referencedTable = foreignKeys[propertyName];
                    classBuilder.AppendLine($"[ForeignKey(\"{referencedTable}\")]");
                }

                // Add additional constraints like [Required], [MaxLength]
                if (constraints != null && constraints.ContainsKey(propertyName))
                {
                    foreach (var constraint in constraints[propertyName])
                    {
                        classBuilder.AppendLine($"{constraint}");
                    }
                }

                classBuilder.AppendLine($"public {csharpType} {propertyName} {{ get; set; }}");
            }

            classBuilder.AppendLine("    }");
            classBuilder.AppendLine("}");
            return classBuilder.ToString();
        }

        


        private string GetCSharpType(string sqlType)
        {
            return sqlType.ToLower() switch
            {
                "varchar" or "nvarchar" => "string",
                "int" => "int",
                "bigint" => "long",
                "decimal" => "decimal",
                "datetime" => "DateTime",
                "bit" => "bool",
                _ => "object"
            };
        }

        private string? GetSolutionDirectory()
        {
            var directory = Directory.GetCurrentDirectory();
            int attempts = 0;

            while (directory != null && attempts < 3)
            {
                directory = Directory.GetParent(directory)?.FullName;
                attempts++;
            }

            return directory;
        }

        private bool ConfirmAction()
        {
            var response = Console.ReadLine()?.Trim().ToLower();
            return response == "y" || response == "yes";
        }
    }
}
