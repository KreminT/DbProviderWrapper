#region

using System.Collections.Generic;
using System.Data.SqlClient;

#endregion

namespace DbProviderWrapper.Persistence
{
    public class SimpleDataTable<TType>
    {
        #region Fields

        private readonly List<TType> _data = new List<TType>();

        private int _index;

        #endregion

        /// <summary>
        ///     Add new row
        /// </summary>
        public void AddNewRow(TType model)
        {
            _data.Add(model);
        }

        public List<TType> GetData()
        {
            return _data;
        }

        public TYpe GetRow<TYpe>()
        {
            object lValue = _data[_index];

            return (TYpe) lValue;
        }

        public bool ReadNext()
        {
            return _index++ < _data.Count;
        }
    }
}