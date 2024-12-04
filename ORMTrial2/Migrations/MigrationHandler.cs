using System;
using Microsoft.Data.SqlClient;
using ORMTrial2.Tools;

namespace ORMTrial2.Migrations
{
    public class MigrationHandler
    {
        private readonly SchemaGenerator _schemaGenerator;
        private readonly DbFrame _dbFrame;

        // Constructor to initialize dependencies
        public MigrationHandler()
        {
            _schemaGenerator = new SchemaGenerator();
            _dbFrame = new DbFrame();
        }

        // Method to handle migrations for all models in DbFrame
        public void HandleMigrations(string connectionString)
        {
            Console.WriteLine("Starting database migrations...");

            // Iterate through all registered models in DbFrame
            foreach (var modelEntry in _dbFrame.Model)
            {
                var tableName = modelEntry.Key;
                var modelType = modelEntry.Value;

                Console.WriteLine($"Checking if table '{tableName}' exists...");

                // Check if the table exists in the database
                bool tableExists = TableExists(tableName, connectionString);

                if (tableExists)
                {
                    // Table exists, compare columns
                    Console.WriteLine($"Table '{tableName}' already exists. Checking for column differences.");

                    // Compare columns and generate ALTER TABLE script if needed
                    _schemaGenerator.GenerateAlterTableScripts(modelType, tableName, connectionString);
                    Console.WriteLine($"Migration applied for table '{tableName}'.");
                }
                else
                {
                    // Table doesn't exist, create the table
                    Console.WriteLine($"Table '{tableName}' does not exist. Creating table.");
                    _schemaGenerator.GenerateCreateTableScript(modelType, tableName, connectionString);
                    Console.WriteLine($"Table '{tableName}' created successfully.");
                }
            }

            Console.WriteLine("Database migrations completed.");
        }

        // Helper method to check if a table exists in the database
        private bool TableExists(string tableName, string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
                var result = command.ExecuteScalar();
                return (int)result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking table existence: {ex.Message}");
                return false;
            }
        }
    }
}
