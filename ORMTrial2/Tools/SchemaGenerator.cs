using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace ORMTrial2.Tools
{
    public class SchemaGenerator
    {
        private readonly QueryGenerator _queryGenerator;

        public SchemaGenerator()
        {
            _queryGenerator = new QueryGenerator();
        }

        // Synchronize tables dynamically using the AppDbContext
        public void SynchronizeTables(AppDbContext appDbContext, string connectionString)
        {
            var modelTypes = appDbContext.Model.GetEntityTypes()
                                .Select(t => t.ClrType)
                                .ToDictionary(t => t.Name, t => t);

            var dbTables = GetTablesFromDatabase(connectionString);

            // Synchronize tables based on models
            foreach (var model in modelTypes)
            {
                var tableName = model.Key;
                var modelType = model.Value;

                if (dbTables.Contains(tableName))
                {
                    // Table exists, check for column differences
                    Console.WriteLine($"Table '{tableName}' already exists. Checking for column differences...");
                    var existingColumns = GetColumnsFromDatabase(tableName, connectionString);
                    GenerateAlterTableScripts(modelType, tableName, existingColumns, connectionString);
                }
                else
                {
                    // Table does not exist, create it
                    Console.WriteLine($"Table '{tableName}' does not exist. Creating...");
                    GenerateCreateTableScript(modelType, tableName, connectionString);
                }
            }

            // Delete tables from the database that are not mapped in the models
            foreach (var dbTable in dbTables)
            {
                if (!modelTypes.ContainsKey(dbTable))
                {
                    Console.WriteLine($"Table '{dbTable}' is not mapped to any model. Deleting...");
                    DropTable(dbTable, connectionString);
                }
            }
        }

        // Generate CREATE TABLE script and execute it
        public void GenerateCreateTableScript(Type modelType, string tableName, string connectionString)
        {
            var columns = GetColumnsFromModel(modelType);
            var sqlScript = _queryGenerator.GenerateCreateTable(tableName, columns);

            Console.WriteLine($"Generated CREATE TABLE Script:\n{sqlScript}");
            ExecuteSqlCommand(sqlScript, connectionString);
        }

        // Generate ALTER TABLE scripts for existing tables and execute them
        public void GenerateAlterTableScripts(Type modelType, string tableName, Dictionary<string, string> existingColumns, string connectionString)
        {
            var newColumns = GetColumnsFromModel(modelType);
            var alterScript = _queryGenerator.GenerateAlterTable(tableName, existingColumns, newColumns);

            if (!string.IsNullOrEmpty(alterScript))
            {
                Console.WriteLine($"Generated ALTER TABLE Script:\n{alterScript}");
                ExecuteSqlCommand(alterScript, connectionString);
            }
            else
            {
                Console.WriteLine($"No changes detected for table '{tableName}'.");
            }
        }

        // Drop a specific table
        public void DropTable(string tableName, string connectionString)
        {
            var dropScript = $"DROP TABLE IF EXISTS [{tableName}];";

            Console.WriteLine($"Generated DROP TABLE Script:\n{dropScript}");
            ExecuteSqlCommand(dropScript, connectionString);
        }

        // Get column definitions from a model type
        private Dictionary<string, string> GetColumnsFromModel(Type modelType)
        {
            var columns = new Dictionary<string, string>();
            var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var columnName = property.Name;
                var columnType = GetSqlColumnType(property.PropertyType);

                if (property.GetCustomAttributes(typeof(KeyAttribute), inherit: false).Any())
                {
                    columnType += " PRIMARY KEY IDENTITY(1,1)";
                }

                columns.Add(columnName, columnType);
            }

            return columns;
        }

        // Get columns for an existing table from the database
        private Dictionary<string, string> GetColumnsFromDatabase(string tableName, string connectionString)
        {
            var columns = new Dictionary<string, string>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var command = new SqlCommand($@"
                    SELECT COLUMN_NAME, DATA_TYPE 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = '{tableName}'", connection);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(0);
                    var columnType = reader.GetString(1);
                    columns.Add(columnName, columnType);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving columns from database: {ex.Message}");
            }

            return columns;
        }

        // Get all table names from the database
        private List<string> GetTablesFromDatabase(string connectionString)
        {
            var tables = new List<string>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var command = new SqlCommand(@"
                    SELECT TABLE_NAME 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_TYPE = 'BASE TABLE'", connection);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving tables from database: {ex.Message}");
            }

            return tables;
        }

        // Get SQL type mapping for C# types
        private string GetSqlColumnType(Type type)
        {
            return type switch
            {
                Type t when t == typeof(int) => "INT",
                Type t when t == typeof(string) => "VARCHAR(255)",
                Type t when t == typeof(DateTime) => "DATETIME",
                Type t when t == typeof(bool) => "BIT",
                _ => "VARCHAR(255)"
            };
        }

        // Execute an SQL command against the database
        private void ExecuteSqlCommand(string sqlScript, string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                using var command = new SqlCommand(sqlScript, connection);
                command.ExecuteNonQuery();

                Console.WriteLine($"SQL command executed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while executing the SQL command: {ex.Message}");
            }
        }
    }
}
