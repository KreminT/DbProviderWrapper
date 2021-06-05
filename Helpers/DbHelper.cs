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

        public static object NullIf(this int value)
        {
            if (value == 0)
                return DBNull.Value;
            return value;
        }

        public static object NullIf(this int? value)
        {
            switch (value)
            {
                case null:
                case 0:
                    return DBNull.Value;
                default:
                    return value.Value;
            }
        }

        public static object NullIf(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return DBNull.Value;
            return value;
        }

        public static object NullIf(this double value)
        {
            if (value == 0)
                return DBNull.Value;
            return value;
        }

        public static object NullIf(double? lValue)
        {
            switch (lValue)
            {
                case null:
                case 0:
                    return DBNull.Value;
                default:
                    return lValue.Value;
            }
        }

        public static object NullIf(this DateTime date)
        {
            if (date < DateTime.Now.AddYears(-20))
                return DBNull.Value;
            return date;
        }

        public static object NullIf(this DateTime? date)
        {
            if (!date.HasValue)
                return DBNull.Value;
            if (date.Value < DateTime.Now.AddYears(-20))
                return DBNull.Value;
            return date.Value;
        }

        public static object NullIf(this bool? value)
        {
            if (value.HasValue)
                return value.Value;
            return DBNull.Value;
        }
    }
}