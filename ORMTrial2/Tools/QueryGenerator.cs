using System;
using System.Collections.Generic;
using System.Linq;

namespace ORMTrial2.Tools
{
    public class QueryGenerator
    {
        // Generates a CREATE TABLE SQL statement from a model
        public string GenerateCreateTable(string tableName, Dictionary<string, string> columns)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

            if (columns == null || !columns.Any())
                throw new ArgumentException("Columns dictionary must have at least one column.", nameof(columns));

            var columnsDefinition = string.Join(", ", columns.Select(c => $"{c.Key} {c.Value}"));
            return $"CREATE TABLE [{tableName}] ({columnsDefinition});";
        }

        // Generates a SELECT statement
        public string GenerateSelect(string tableName, List<string> columns = null, string whereClause = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

            var columnsPart = columns != null && columns.Any() ? string.Join(", ", columns) : "*";
            var wherePart = string.IsNullOrWhiteSpace(whereClause) ? string.Empty : $" WHERE {whereClause}";

            return $"SELECT {columnsPart} FROM [{tableName}]{wherePart};";
        }

        // Generates an INSERT statement
        public string GenerateInsert(Type modelType, string tableName, Dictionary<string, object> data, List<string> excludeColumns = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

            if (data == null || !data.Any())
                throw new ArgumentException("Data dictionary must have at least one entry.", nameof(data));

            // Exclude columns if specified
            if (excludeColumns != null)
            {
                data = data
                    .Where(kvp => !excludeColumns.Contains(kvp.Key)) // Exclude specified columns
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            // Exclude properties marked with [Key] attribute
            if (modelType != null)
            {
                var propertiesWithKey = modelType.GetProperties()
                                                 .Where(p => Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)))
                                                 .Select(p => p.Name)
                                                 .ToList();

                data = data
                    .Where(kvp => !propertiesWithKey.Contains(kvp.Key)) // Exclude properties with [Key] attribute
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            if (!data.Any())
                throw new ArgumentException("Data dictionary cannot be empty after excluding columns and [Key] properties.", nameof(data));

            // Generate columns and values
            var columns = string.Join(", ", data.Keys); // Column names
            var values = string.Join(", ", data.Values.Select(value =>
            {
                if (value == null)
                    return "NULL"; // Handle null values explicitly as NULL in SQL

                return value switch
                {
                    string or char => $"'{value}'", // Enclose strings and chars in single quotes
                    DateTime dateTime => $"'{dateTime:yyyy-MM-dd HH:mm:ss}'", // Format DateTime for SQL
                    bool boolValue => boolValue ? "1" : "0", // Convert boolean to 1 (True) or 0 (False) for SQL
                    _ => value.ToString() // Use the value directly for numbers and other types
                };
            }));


            return $"INSERT INTO [{tableName}] ({columns}) VALUES ({values});";
        }




        // Generates an UPDATE statement
        public string GenerateUpdate(string tableName, Dictionary<string, object> data, string whereClause)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

            if (data == null || !data.Any())
                throw new ArgumentException("Data dictionary must have at least one entry.", nameof(data));

            if (string.IsNullOrWhiteSpace(whereClause))
                throw new ArgumentException("Where clause cannot be null or empty for updates.", nameof(whereClause));

            var setClause = string.Join(", ", data.Keys.Select(k => $"{k} = @{k}"));
            return $"UPDATE {tableName} SET {setClause} WHERE {whereClause};";
        }

        // Generates a DELETE statement
        public string GenerateDelete(string tableName, string whereClause)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

            if (string.IsNullOrWhiteSpace(whereClause))
                throw new ArgumentException("Where clause cannot be null or empty for deletions.", nameof(whereClause));

            return $"DELETE FROM {tableName} WHERE {whereClause};";
        }

        // Generates an ALTER TABLE statement (for adding new columns)
        public string GenerateAlterTable(string tableName, Dictionary<string, string> existingColumns, Dictionary<string, string> newColumns)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

            if (existingColumns == null)
                throw new ArgumentException("Existing columns dictionary cannot be null.", nameof(existingColumns));

            if (newColumns == null)
                throw new ArgumentException("New columns dictionary cannot be null.", nameof(newColumns));

            var alterTableScripts = new List<string>();

            // Check for columns to add
            foreach (var newColumn in newColumns)
            {
                if (!existingColumns.ContainsKey(newColumn.Key))
                {
                    alterTableScripts.Add($"ALTER TABLE [{tableName}] ADD {newColumn.Key} {newColumn.Value}");
                }
            }

            // Check for columns to modify
            foreach (var newColumn in newColumns)
            {
                if (existingColumns.ContainsKey(newColumn.Key) && existingColumns[newColumn.Key] != newColumn.Value)
                {
                    alterTableScripts.Add($"ALTER TABLE [{tableName}] ALTER COLUMN {newColumn.Key} {newColumn.Value}");
                }
            }

            // Check for columns to drop
            foreach (var existingColumn in existingColumns)
            {
                if (!newColumns.ContainsKey(existingColumn.Key))
                {
                    alterTableScripts.Add($"ALTER TABLE [{tableName}] DROP COLUMN {existingColumn.Key}");
                }
            }

            if (!alterTableScripts.Any())
            {
                return null; // No changes to apply
            }

            return string.Join(Environment.NewLine, alterTableScripts);
        }
    }
}
