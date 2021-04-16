using System.Collections.Generic;
using DbProviderWrapper.Interfaces;

namespace DbProviderWrapper.Persistence
{
    public class SqlQueueItem:ISqlQueueItem
    {
        #region Fields

        private IEnumerable<ISqlParameter> _parameters;
        private string _commandName;

        #endregion

        #region Constructors

        public SqlQueueItem(IEnumerable<ISqlParameter> parameters, string commandName)
        {
            _parameters = parameters;
            _commandName = commandName;
        }

        #endregion

        #region Properties

        public IEnumerable<ISqlParameter> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public string CommandName
        {
            get { return _commandName; }
            set { _commandName = value; }
        }

        #endregion
    }
}