using Microsoft.Extensions.Configuration;
using System.IO;

namespace ORMTrial2.Utils
{
    public static class ConfigLoader
    {
        public static IConfiguration LoadConfig(string filePath)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) 
                .AddJsonFile(filePath, optional: false, reloadOnChange: true);

            var config = configBuilder.Build();

            // Debugging the loaded connection string
            string connectionString = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Error: Connection string is null or empty.");
            }
            else
            {
                Console.WriteLine($"Connection string loaded: {connectionString}");
            }

            return config;
        }
        
    }
}
