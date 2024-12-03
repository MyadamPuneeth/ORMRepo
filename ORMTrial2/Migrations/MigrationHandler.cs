using System;
using Microsoft.Data.SqlClient;
using ORMTrial2.Tools;

namespace ORMTrial2.Migrations
{
    public class MigrationHandler
    {
        private readonly SchemaGenerator _schemaGenerator;

        // Constructor to initialize dependencies
        public MigrationHandler()
        {
            _schemaGenerator = new SchemaGenerator();
        }

        // Method to check if a table exists and apply migration or create table
        public void HandleMigration(Type modelType, string connectionString)
        {
            Console.WriteLine($"Checking if table '{modelType.Name}' exists...");

            // Check if the table exists in the database
            bool tableExists = TableExists(modelType.Name, connectionString);

            if (tableExists)
            {
                // Table exists, compare columns
                Console.WriteLine($"Table '{modelType.Name}' already exists. Checking for column differences.");

                // Compare columns and generate ALTER TABLE script if needed
                var migrationScripts = _schemaGenerator.GenerateAlterTableScripts(modelType, connectionString);
                Console.WriteLine($"Migration applied for table '{modelType.Name}'.");
                
            }
            else
            {
                // Table doesn't exist, create the table
                Console.WriteLine($"Table '{modelType.Name}' does not exist. Creating table.");
                var schema = _schemaGenerator.GenerateCreateTableScript(modelType, connectionString);
                Console.WriteLine($"Table '{modelType.Name}' created successfully.");
            }
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
