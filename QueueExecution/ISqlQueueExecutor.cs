using System.Collections.Generic;
using System.Threading.Tasks;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.QueueExecution
{
    public interface ISqlQueueExecutor
    {
        void AddToQueue<T>(ISqlQueued<T> persistence, T model) where T : IObjectState;
        void AddToQueue(string procedure, IEnumerable<ISqlParameter> parameters);
        Task<bool> Execute();
    }
}