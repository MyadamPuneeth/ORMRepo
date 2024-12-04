using System.Collections.Concurrent;
using System.Reflection;

namespace ORMTrial2.Tools
{
    public class DbFrame
    {
        private readonly ConcurrentDictionary<string, Type> _models = new();

        public DbFrame()
        {
            // Automatically register all DbSet properties as models
            RegisterModels();
        }

        // Expose the registered models as a read-only dictionary
        public IReadOnlyDictionary<string, Type> Model => _models;

        private void RegisterModels()
        {
            // Get all properties of type DbSet<T> in the derived class
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(DbFrame<>));

            foreach (var property in properties)
            {
                var tableName = NormalizeTableName(property.Name);
                var modelType = property.PropertyType.GetGenericArguments().First();

                if (!_models.TryAdd(tableName, modelType))
                {
                    throw new InvalidOperationException($"The model '{tableName}' is already registered.");
                }
            }
        }

        private string NormalizeTableName(string name)
        {
            // Normalize table name to PascalCase (or customize as needed)
            return name;
        }
    }

    // Placeholder for DbSet implementation
    public class DbFrame<T> { }
}
