using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class DBFrame
{
    private readonly ConcurrentDictionary<string, Type> _models = new();

    public DBFrame()
    {
        // Automatically register all DbSet properties as models
        RegisterModels();
    }
    public IReadOnlyDictionary<string, Type> Models => _models;

    public IEnumerable<Type> GetEntityTypes()
    {
        return _models.Values;
    }

    private void RegisterModels()
    {
        // Get all properties of type DbSet<T> in the derived class
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

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

public class DbSet<T>
{
    // Placeholder for DbSet implementation
}
