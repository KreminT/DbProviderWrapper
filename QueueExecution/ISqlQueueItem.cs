using System.Collections.Generic;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.QueueExecution
{
    public interface ISqlQueueItem
    {
        #region Properties

        IEnumerable<ISqlParameter> Parameters { get; }
        string CommandName { get; }

        #endregion
    }
}