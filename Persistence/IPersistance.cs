using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.Persistence
{
    public interface IPersistence<TType>
    {
        bool Delete(TType model);
        Task<bool> DeleteAsync(TType model);

        SimpleDataTable<TType> Load(List<ISqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<SimpleDataTable<TType>> LoadAsync(List<ISqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        SimpleDataTable<TType> Save(TType model);
        Task<SimpleDataTable<TType>> SaveAsync(TType model);
        SimpleDataTable<TType> Update(TType model);
        Task<SimpleDataTable<TType>> UpdateAsync(TType model);
    }
}