#region

using System.Collections.Generic;
using System.Data.SqlClient;

#endregion

namespace DbProviderWrapper.Persistance
{
    public class SimpleDataTable<TTYPE>
    {
        #region Fields

        private readonly List<string> _columns = new List<string>();

        private readonly List<TTYPE> _data = new List<TTYPE>();

        private int _index;

        #endregion

        /// <summary>
        ///     Add new row
        /// </summary>
        public void AddNewRow(TTYPE model)
        {
            _data.Add(model);
        }

        public void BindColumns(List<SqlParameter> columns)
        {
            foreach (SqlParameter lSqlParameter in columns)
                _columns.Add(lSqlParameter.ParameterName);
        }

        public List<TTYPE> GetData()
        {
            return _data;
        }

        public TYPE GetRow<TYPE>()
        {
            object lValue = _data[_index];

            return (TYPE) lValue;
        }

        public bool ReadNext()
        {
            return _index++ < _data.Count;
        }
    }
}