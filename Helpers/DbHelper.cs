#region

using System;
using System.Data.SqlClient;

#endregion

namespace DbProviderWrapper.Helpers
{
    public static class DbHelper
    {
        public static TType GetValue<TType>(this SqlDataReader sqlDataReader, string name)
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

        public static bool ColumnExists(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                if (reader.GetName(i) == columnName)
                    return true;

            return false;
        }
    }
}