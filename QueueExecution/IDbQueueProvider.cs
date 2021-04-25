using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.QueueExecution
{
    public interface IDbQueueProvider
    {
        DbConnection CreateConnection();

        Task<bool> StoredProcAsync(string procedureName, IEnumerable<ISqlParameter> parameters,
            DbConnection connection, DbTransaction transaction);

        Task DisposeConnectionAsync(DbTransaction transaction, DbConnection sqlConnection);
    }
}