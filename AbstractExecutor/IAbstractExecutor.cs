using System.Threading.Tasks;
using DbProviderWrapper.Persistence;

namespace DbProviderWrapper.AbstractExecutor
{
    public interface IAbstractExecutor
    {
        Task<SimpleDataTable<TTypeResult>> Execute<TTypeResult, TTypeArgs>(string commandName, TTypeArgs args);
    }
}