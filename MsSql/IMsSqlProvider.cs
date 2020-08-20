using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DbProviderWrapper.Interfaces;
using DbProviderWrapper.Persistance;

namespace DbProviderWrapper.MsSql
{
    public interface IMsSqlProvider
    {
        public IDbLogger Logger { get; }
        List<IDictionary<string, object>> Select(string query, IEnumerable<string> columns);
        object SimpleSelect(string query);
        SqlDataReader StoredProc(string procedureName, IEnumerable<SqlParameter> sqlParameters);
        Task<SqlDataReader> StoredProcAsync(string procedureName, IEnumerable<SqlParameter> sqlParameters);
        void StoredProc<TTYPE>(
        string procedureName,
        IEnumerable<SqlParameter> sqlParameters,
        ref SimpleDataTable<TTYPE> simpleDataTable,
            Func<SqlDataReader, TTYPE> LoadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<SimpleDataTable<TTYPE>> StoredProcAsync<TTYPE>(
            string procedureName,
            IEnumerable<SqlParameter> sqlParameters,
            SimpleDataTable<TTYPE> simpleDataTable,
            Func<SqlDataReader, TTYPE> LoadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    }
}