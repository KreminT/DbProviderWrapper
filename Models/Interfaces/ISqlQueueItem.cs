using System.Collections.Generic;

namespace DbProviderWrapper.Models.Interfaces
{
    public interface ISqlQueueItem
    {
        #region Properties

        IEnumerable<ISqlParameter> Parameters { get; }
        string CommandName { get; }

        #endregion
    }
}