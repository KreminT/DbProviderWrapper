using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DbProviderWrapper.Persistence;

namespace DbProviderWrapper.MsSql
{
    public interface IMsSqlPersistence<TType>
    {
        bool Delete(TType model);
        Task<bool> DeleteAsync(TType model);

        SimpleDataTable<TType> Load(List<SqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<SimpleDataTable<TType>> LoadAsync(List<SqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        SimpleDataTable<TType> Save(TType model);
        Task<SimpleDataTable<TType>> SaveAsync(TType model);
        SimpleDataTable<TType> Update(TType model);
        Task<SimpleDataTable<TType>> UpdateAsync(TType model);
    }
}