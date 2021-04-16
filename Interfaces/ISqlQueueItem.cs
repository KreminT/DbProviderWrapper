using System.Collections.Generic;

namespace DbProviderWrapper.Interfaces
{
    public interface ISqlQueueItem
    {
        #region Properties

        IEnumerable<ISqlParameter> Parameters { get; }
        string CommandName { get; }

        #endregion
    }
}