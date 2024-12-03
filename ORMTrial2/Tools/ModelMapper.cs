using System;
using System.Collections.Generic;
using System.Data;

namespace ORMTrial2.Tools
{
    public class ModelMapper
    {
        // Maps a data reader's current row to a dictionary representing column-value pairs
        public Dictionary<string, object> MapToDictionary(IDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var row = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[columnName] = value;
            }

            return row;
        }

        // Maps a data reader's rows to a list of strongly typed models
        public List<T> MapToList<T>(IDataReader reader) where T : new()
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var result = new List<T>();
            var properties = typeof(T).GetProperties();

            while (reader.Read())
            {
                var instance = new T();
                foreach (var prop in properties)
                {
                    var columnName = prop.Name;
                    if (!reader.HasColumn(columnName)) continue;

                    var value = reader[columnName];
                    if (value == DBNull.Value) value = null;

                    prop.SetValue(instance, value);
                }
                result.Add(instance);
            }

            return result;
        }

        // Maps a single row from the data reader to a strongly typed model
        public T MapToModel<T>(IDataReader reader) where T : new()
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (!reader.Read()) return default;

            var instance = new T();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                var columnName = prop.Name;
                if (!reader.HasColumn(columnName)) continue;

                var value = reader[columnName];
                if (value == DBNull.Value) value = null;

                prop.SetValue(instance, value);
            }

            return instance;
        }
    }

    public static class DataReaderExtensions
    {
        // Extension method to check if a column exists in the data reader
        public static bool HasColumn(this IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
