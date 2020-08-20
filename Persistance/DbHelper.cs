#region

using System;
using System.Data.SqlClient;

#endregion

namespace DbProviderWrapper.Persistance
{
    public static class DbHelper
    {
        public static TTYPE GetValue<TTYPE>(this SqlDataReader sqlDataReader, string name)
        {
            object lValue = sqlDataReader[name];

            if (lValue == DBNull.Value)
                return default;

            if (typeof(TTYPE) == typeof(int) || typeof(TTYPE) == typeof(int?))
                return (TTYPE) (object) Convert.ToInt32(lValue);

            if (lValue == DBNull.Value || !(lValue is TTYPE))
                return default;

            return (TTYPE) lValue;
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