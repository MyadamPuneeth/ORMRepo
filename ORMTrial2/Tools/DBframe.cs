using System.Collections.Concurrent;
using System.Reflection;

namespace ORMTrial2.Tools
{
    public class DbFrame
    {
        private readonly ConcurrentDictionary<string, Type> _models = new();

        public IReadOnlyDictionary<string, Type> Model => _models;
    }

    // Placeholder for DbSet implementation
    public class DbFrame<T> { }
}
