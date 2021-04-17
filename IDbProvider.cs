using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Models.Interfaces;
using DbProviderWrapper.Persistence;

namespace DbProviderWrapper
{
    public interface IDbProvider
    {
        IDbLogger Logger { get; }
        List<IDictionary<string, object>> Select(string query, IEnumerable<string> columns);
        object SimpleSelect(string query);
        IDataReader StoredProc(string procedureName, IEnumerable<ISqlParameter> sqlParameters);
        Task<IDataReader> StoredProcAsync(string procedureName, IEnumerable<ISqlParameter> sqlParameters);
        void StoredProc<TType>(
        string procedureName,
        IEnumerable<ISqlParameter> sqlParameters,
        ref SimpleDataTable<TType> simpleDataTable,
            Func<IDataReader, TType> loadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<SimpleDataTable<TType>> StoredProcAsync<TType>(
            string procedureName,
            IEnumerable<ISqlParameter> sqlParameters,
            SimpleDataTable<TType> simpleDataTable,
            Func<IDataReader, TType> loadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    }
}