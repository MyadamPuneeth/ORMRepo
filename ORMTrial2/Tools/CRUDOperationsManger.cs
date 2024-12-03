using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ORMTrial2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMTrial2.Tools
{
    public class CRUDOperationsManger
    {
        private readonly QueryGenerator _queryGenerator;
        private readonly SchemaGenerator _schemaGenerator;
        

        public CRUDOperationsManger()
        {
            _queryGenerator = new QueryGenerator();
            var config = ConfigLoader.LoadConfig("appsettings.json");
            string connectionString = config.GetConnectionString("DefaultConnection");
        }

        public void InsertData<T>(Type modelType, string tableName, T entity, string connectionString)
        {
            // Convert the entity's properties to a dictionary
            var data = entity.GetType()
                             .GetProperties()
                             .ToDictionary(prop => prop.Name, prop => prop.GetValue(entity) ?? DBNull.Value);

            // Generate the insert query
            var insertQuery = _queryGenerator.GenerateInsert(modelType,tableName, data);

            // Execute the query
            ExecuteCommand(insertQuery, data, connectionString);
            Console.WriteLine("Data inserted successfully.");
        }

        public void UpdateData<T>(string tableName, T entity, string whereClause, string connectionString) where T : class
        {
            // Convert the entity's properties to a dictionary
            var data = entity.GetType()
                             .GetProperties()
                             .ToDictionary(prop => prop.Name, prop => prop.GetValue(entity) ?? DBNull.Value);

            // Generate the update query
            var updateQuery = _queryGenerator.GenerateUpdate(tableName, data, whereClause);

            // Execute the query
            ExecuteCommand(updateQuery, data, connectionString);
            Console.WriteLine("Data updated successfully.");
        }

        public void DeleteData(string tableName, string whereClause, string connectionString)
        {
            // Generate the delete query
            var deleteQuery = _queryGenerator.GenerateDelete(tableName, whereClause);

            // Execute the query
            ExecuteCommand(deleteQuery, null, connectionString);
            Console.WriteLine("Data deleted successfully.");
        }

        public List<Dictionary<string, object>> SelectData(string tableName, List<string> columns = null, string whereClause = null)
        {
            // Generate the select query
            var selectQuery = _queryGenerator.GenerateSelect(tableName, columns, whereClause);

            // Execute the query and fetch results
            var results = FetchResults(selectQuery);

            Console.WriteLine("Data fetched successfully.");
            return results;
        }

        private List<Dictionary<string, object>> FetchResults(string query)
        {
            var results = new List<Dictionary<string, object>>();

            using (var connection = new SqlConnection(ConfigLoader.LoadConfig("appsettings.json").GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                    }
                }
            }

            return results;
        }


        private void ExecuteCommand(string query, Dictionary<string, object> parameters, string connectionString)
        {
            // Implement logic for executing SQL command here
            Console.WriteLine($"Executing Query: {query}");
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                using var command = new SqlCommand(query, connection);
                var rowsAffected = command.ExecuteNonQuery();

                Console.WriteLine($"SQL command executed successfully. Rows affected: {rowsAffected}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while executing the SQL command: {ex.Message}");
            }
            // Add database execution logic (e.g., using ADO.NET)
        }

    }
}
