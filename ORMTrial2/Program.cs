using ORMTrial2.Tools;
using Microsoft.Extensions.Configuration;
using ORMTrial2.Utils;

namespace ORMTrial2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load configuration to get the connection string
            var config = ConfigLoader.LoadConfig("appsettings.json");
            string connectionString = config.GetConnectionString("DefaultConnection");

            // Initialize necessary components
            var schemaGenerator = new SchemaGenerator();
            var dbContext = new AppDbContext(); // Instantiate the AppDbContext
            dbContext.CRUDopsMethod();
            var modelGenerator = new ModelGenerator();
            //modelGenerator.GenerateModels(connectionString);

            // Call the SynchronizeTables method
            Console.WriteLine("Do you want to synchronize all table schemas? (yes/no)");
            string input = Console.ReadLine();

            if (input?.ToLower() == "yes")
            {
                schemaGenerator.SynchronizeTables(dbContext, connectionString);
                Console.WriteLine("All table schemas synchronized successfully.");
            }
            else
            {
                Console.WriteLine("Skipping table schema synchronization.");
            }
        }
    }
}
