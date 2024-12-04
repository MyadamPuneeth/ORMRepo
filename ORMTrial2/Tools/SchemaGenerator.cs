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

        // Synchronize tables dynamically using DbFrame
        public void SynchronizeTables(DbFrame dbFrame, string connectionString)
        {
            Console.WriteLine("Starting table synchronization...");

            // Get models from DbFrame
            var modelDefinitions = dbFrame.GetType()
                                           .GetProperties()
                                           .Where(prop => prop.PropertyType.IsGenericType &&
                                                          prop.PropertyType.GetGenericTypeDefinition() == typeof(DbFrame<>))
                                           .Select(prop => new
                                           {
                                               TableName = prop.Name,
                                               ModelType = prop.PropertyType.GenericTypeArguments[0]
                                           })
                                           .ToList();

            // Fetch existing tables from the database
            var dbTables = GetTablesFromDatabase(connectionString);

            // Synchronize tables
            foreach (var model in modelDefinitions)
            {
                var tableName = model.TableName;
                var modelType = model.ModelType;

                if (dbTables.Contains(tableName))
                {
                    // Table exists: Generate ALTER scripts
                    Console.WriteLine($"Table '{tableName}' exists. Generating ALTER scripts...");
                    GenerateAlterTableScripts(modelType, tableName, connectionString);
                }
                else
                {
                    // Table does not exist: Create it
                    Console.WriteLine($"Table '{tableName}' does not exist. Creating table...");
                    GenerateCreateTableScript(modelType, tableName, connectionString);
                }
            }

            // Drop tables from the database that are not mapped in DbFrame
            var modelTableNames = new HashSet<string>(modelDefinitions.Select(m => m.TableName));
            foreach (var dbTable in dbTables)
            {
                if (!modelTableNames.Contains(dbTable))
                {
                    Console.WriteLine($"Table '{dbTable}' is not mapped in DbFrame. Dropping...");
                    GenerateDropTableScript(dbTable, connectionString);
                }
            }

            Console.WriteLine("Table synchronization complete.");
        }

        // Generate CREATE TABLE script
        public void GenerateCreateTableScript(Type modelType, string tableName, string connectionString)
        {
            var columns = GetColumnsFromModel(modelType);
            var sqlScript = _queryGenerator.GenerateCreateTable(tableName, columns);

            Console.WriteLine($"Generated CREATE TABLE script:\n{sqlScript}");
            ExecuteSqlCommand(sqlScript, connectionString);
        }

        // Generate ALTER TABLE scripts
        public void GenerateAlterTableScripts(Type modelType, string tableName, string connectionString)
        {
            var existingColumns = GetExistingColumnsFromDatabase(tableName, connectionString);
            var newColumns = GetColumnsFromModel(modelType);
            var alterScript = _queryGenerator.GenerateAlterTable(tableName, existingColumns, newColumns);

            if (!string.IsNullOrEmpty(alterScript))
            {
                Console.WriteLine($"Generated ALTER TABLE script:\n{alterScript}");
                ExecuteSqlCommand(alterScript, connectionString);
            }
            else
            {
                Console.WriteLine($"No changes detected for table '{tableName}'.");
            }
        }

        private Dictionary<string, string> GetExistingColumnsFromDatabase(string tableName, string connectionString)
        {
            var existingColumns = new Dictionary<string, string>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = @"
            SELECT 
                COLUMN_NAME, 
                DATA_TYPE 
            FROM 
                INFORMATION_SCHEMA.COLUMNS 
            WHERE 
                TABLE_NAME = @TableName";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var columnName = reader["COLUMN_NAME"].ToString();
                            var dataType = reader["DATA_TYPE"].ToString();

                            existingColumns[columnName] = dataType;
                        }
                    }
                }
            }

            return existingColumns;
        }

        // Drop a table
        public void GenerateDropTableScript(string tableName, string connectionString)
        {
            var dropScript = $"DROP TABLE IF EXISTS [{tableName}];";

            Console.WriteLine($"Generated DROP TABLE script:\n{dropScript}");
            ExecuteSqlCommand(dropScript, connectionString);
        }

        // Map model properties to SQL column definitions
        private Dictionary<string, string> GetColumnsFromModel(Type modelType)
        {
            var columns = new Dictionary<string, string>();
            var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var columnName = property.Name;
                var columnType = GetSqlColumnType(property.PropertyType);

                // Check for [Key] attribute (Primary Key)
                if (property.GetCustomAttributes(typeof(KeyAttribute), inherit: false).Any())
                {
                    columnType += " PRIMARY KEY IDENTITY(1,1)";
                }

                columns.Add(columnName, columnType);
            }

            return columns;
        }

        // Fetch column definitions from the database for a table
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

        // Fetch all table names from the database
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

        // Map C# types to SQL types
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

        // Execute SQL command
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
