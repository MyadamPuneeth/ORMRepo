using ORMTrial2.Tools;
using Microsoft.Data.SqlClient;
using ORMTrial2.Models;
using Microsoft.Extensions.Configuration;
using ORMTrial2.Utils;
using ORMTrial2.Migrations;

namespace ORMTrial2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load configuration using ConfigLoader to get connection string
            var config = ConfigLoader.LoadConfig("appsettings.json");
            string connectionString = config.GetConnectionString("DefaultConnection");
            var manager = new CRUDOperationsManger();

            // Initialize necessary components
            var schemaGenerator = new SchemaGenerator();
            var migrationHandler = new MigrationHandler();

            // Get the database connection using the connection string
            using (var dbConnection = new DatabaseConnection())
            {
                Console.WriteLine("Do you want to apply migrations? (yes/no)");
                string input = Console.ReadLine();

                // Dynamically get model type (in this case, User.cs model)
                Type modelType = typeof(Vattikuti);
                schemaGenerator.GenerateDropTableScript(modelType, connectionString);




                // If user agrees, generate the schema and apply migration
                //if (input?.ToLower() == "yes")
                //{
                //    // Handle migration based on whether the table exists or not
                //    migrationHandler.HandleMigration(modelType, connectionString);
                //}
                //else
                //{
                //    Console.WriteLine("Skipping migrations.");
                //}

                //var user = new student
                //{
                //    stuname = "Vedu",
                //    rollNUmber = 43
                //};
                //manager.InsertData(modelType, "student", user, connectionString);

            }
        }
    }
}
