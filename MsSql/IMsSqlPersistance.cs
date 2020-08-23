using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DbProviderWrapper.Persistance;

namespace DbProviderWrapper.MsSql
{
    public interface IMsSqlPersistance<TTYPE>
    {
        bool Delete(TTYPE model);
        Task<bool> DeleteAsync(TTYPE model);

        SimpleDataTable<TTYPE> Load(List<SqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<SimpleDataTable<TTYPE>> LoadAsync(List<SqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        SimpleDataTable<TTYPE> Save(TTYPE model);
        Task<SimpleDataTable<TTYPE>> SaveAsync(TTYPE model);
        SimpleDataTable<TTYPE> Update(TTYPE model);
        Task<SimpleDataTable<TTYPE>> UpdateAsync(TTYPE model);

        SimpleDataTable<TTYPE> ImportTable(DataTable dataTable);
        Task<SimpleDataTable<TTYPE>> ImportTableAsync(DataTable dataTable);

    }
}