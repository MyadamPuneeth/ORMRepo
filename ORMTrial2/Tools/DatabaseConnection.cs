using Microsoft.Data.SqlClient;
using System;
using System.Data;
using ORMTrial2.Utils;
using Microsoft.Extensions.Configuration;

namespace ORMTrial2.Tools
{
    public class DatabaseConnection : IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection _connection;

        // Constructor to initialize with the connection string
        public DatabaseConnection()
        {
            var config = ConfigLoader.LoadConfig("appsettings.json");
            string connectionString = config.GetConnectionString("DefaultConnection");
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // Opens the database connection
        public void OpenConnection()
        {
            if (_connection == null)
                _connection = new SqlConnection(_connectionString);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        // Closes the database connection
        public void CloseConnection()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        // Executes a SQL command and returns a data reader for queries
        public SqlDataReader ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            OpenConnection();
            using var command = new SqlCommand(query, _connection);
            if (parameters != null)
                command.Parameters.AddRange(parameters);

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        // Executes a non-query SQL command (INSERT, UPDATE, DELETE, etc.)
        public int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            OpenConnection();
            using var command = new SqlCommand(query, _connection);
            if (parameters != null)
                command.Parameters.AddRange(parameters);

            return command.ExecuteNonQuery();
        }

        // Execute a scalar query (e.g., COUNT, MAX, etc.)
        public object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            OpenConnection();
            using var command = new SqlCommand(query, _connection);
            if (parameters != null)
                command.Parameters.AddRange(parameters);

            return command.ExecuteScalar();
        }

        // Implements IDisposable to clean up resources
        public void Dispose()
        {
            CloseConnection();
        }

        // Static method to return a new connection (if needed)
        public static SqlConnection GetConnection()
        {
            var config = ConfigLoader.LoadConfig("appsettings.json");
            string connectionString = config.GetConnectionString("DefaultConnection");
            return new SqlConnection(connectionString);
        }
    }
}
