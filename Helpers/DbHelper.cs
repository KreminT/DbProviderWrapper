#region

using System;
using System.Data;
#endregion

namespace DbProviderWrapper.Helpers
{
    public static class DbHelper
    {
        public static TType GetValue<TType>(this IDataReader sqlDataReader, string name)
        {
            object lValue = sqlDataReader[name];

            if (lValue == DBNull.Value)
                return default;

            if (typeof(TType) == typeof(int) || typeof(TType) == typeof(int?))
                return (TType) (object) Convert.ToInt32(lValue);

            if (lValue == DBNull.Value || !(lValue is TType))
                return default;

            return (TType) lValue;
        }

        public static bool ColumnExists(IDataReader reader, string columnName)
        {
            for (int lI = 0; lI < reader.FieldCount; lI++)
                if (reader.GetName(lI) == columnName)
                    return true;

            return false;
        }
    }
}