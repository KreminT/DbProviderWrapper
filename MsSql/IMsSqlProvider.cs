using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DbProviderWrapper.Interfaces;
using DbProviderWrapper.Persistence;

namespace DbProviderWrapper.MsSql
{
    public interface IMsSqlProvider
    {
        IDbLogger Logger { get; }
        List<IDictionary<string, object>> Select(string query, IEnumerable<string> columns);
        object SimpleSelect(string query);
        SqlDataReader StoredProc(string procedureName, IEnumerable<SqlParameter> sqlParameters);
        Task<SqlDataReader> StoredProcAsync(string procedureName, IEnumerable<SqlParameter> sqlParameters);
        void StoredProc<TType>(
        string procedureName,
        IEnumerable<SqlParameter> sqlParameters,
        ref SimpleDataTable<TType> simpleDataTable,
            Func<SqlDataReader, TType> loadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<SimpleDataTable<TType>> StoredProcAsync<TType>(
            string procedureName,
            IEnumerable<SqlParameter> sqlParameters,
            SimpleDataTable<TType> simpleDataTable,
            Func<SqlDataReader, TType> loadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    }
}