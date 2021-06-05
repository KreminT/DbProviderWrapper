#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DbProviderWrapper.Models.Interfaces;
using DbProviderWrapper.QueueExecution;
using Microsoft.Extensions.Logging;

#endregion

namespace DbProviderWrapper.Persistence
{
    public abstract class Persistence<TType> : IPersistence<TType>, ISqlQueued<TType>
    {
        #region Fields

        private readonly string _strDeleteCommand;
        private readonly string _strLoadCommand;
        private readonly string _strSaveCommand;
        private readonly string _strUpdateCommand;

        private readonly IDbProvider _msSqlProvider;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        protected Persistence(
            string strLoadCommand,
            string strSaveCommand,
            string strUpdateCommand,
            string strDeleteCommand,
            IDbProvider msSqlProvider, ILogger logger)
        {
            _strLoadCommand = strLoadCommand;
            _strSaveCommand = strSaveCommand;
            _strUpdateCommand = strUpdateCommand;
            _strDeleteCommand = strDeleteCommand;
            _msSqlProvider = msSqlProvider;
            _logger = logger;
        }

        #endregion

        public virtual bool Delete(TType model)
        {
            try
            {
                _msSqlProvider.StoredProc(_strDeleteCommand, DeleteModel(model));

                return true;
            }
            catch (Exception lException)
            {
                _logger.LogError("Delete error.", lException);
            }

            return false;
        }

        public virtual async Task<bool> DeleteAsync(TType model)
        {
            try
            {
                await _msSqlProvider.StoredProcAsync(_strDeleteCommand, DeleteModel(model));

                return true;
            }
            catch (Exception lException)
            {
                _logger.LogError("Delete error.", lException);
            }

            return false;
        }

        public virtual SimpleDataTable<TType> Load(List<ISqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            SimpleDataTable<TType> lSimpleDataTable = new SimpleDataTable<TType>();

            _msSqlProvider
                .StoredProc(_strLoadCommand, parameters, ref lSimpleDataTable, LoadModel,
                    isolationLevel);

            return lSimpleDataTable;
        }

        public virtual async Task<SimpleDataTable<TType>> LoadAsync(List<ISqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            SimpleDataTable<TType> lSimpleDataTable = new SimpleDataTable<TType>();

            lSimpleDataTable = await _msSqlProvider
                .StoredProcAsync(_strLoadCommand, parameters, lSimpleDataTable, LoadModel,
                    isolationLevel);

            return lSimpleDataTable;
        }

        public virtual SimpleDataTable<TType> Save(TType model)
        {
            SimpleDataTable<TType> lSimpleDataTable = new SimpleDataTable<TType>();
            try
            {
                TType lInstance = Activator.CreateInstance<TType>();
                List<ISqlParameter> lSqlParameters = SaveModel(model);
                _msSqlProvider
                    .StoredProc(_strSaveCommand, lSqlParameters, ref lSimpleDataTable,
                        LoadModel);
            }
            catch (Exception lException)
            {
                _logger.LogError("Save error.", lException);
                return null;
            }

            return lSimpleDataTable;
        }

        public virtual async Task<SimpleDataTable<TType>> SaveAsync(TType model)
        {
            SimpleDataTable<TType> lSimpleDataTable = new SimpleDataTable<TType>();
            try
            {
                TType lInstance = Activator.CreateInstance<TType>();
                List<ISqlParameter> lSqlParameters = SaveModel(model);
                lSimpleDataTable = await _msSqlProvider
                    .StoredProcAsync(_strSaveCommand, lSqlParameters, lSimpleDataTable,
                        LoadModel);
            }
            catch (Exception lException)
            {
                _logger.LogError("Save error.", lException);
                return null;
            }

            return lSimpleDataTable;
        }

        public virtual SimpleDataTable<TType> Update(TType model)
        {
            SimpleDataTable<TType> lSimpleDataTable = new SimpleDataTable<TType>();
            try
            {
                TType lInstance = Activator.CreateInstance<TType>();
                List<ISqlParameter> lSqlParameters = UpdateModel(model);
                _msSqlProvider
                    .StoredProc(_strUpdateCommand, lSqlParameters, ref lSimpleDataTable,
                        LoadModel);
            }
            catch (Exception lException)
            {
                _logger.LogError("Update error.", lException);
                return null;
            }

            return lSimpleDataTable;
        }

        public virtual async Task<SimpleDataTable<TType>> UpdateAsync(TType model)
        {
            SimpleDataTable<TType> lSimpleDataTable = new SimpleDataTable<TType>();
            try
            {
                TType lInstance = Activator.CreateInstance<TType>();
                List<ISqlParameter> lSqlParameters = UpdateModel(model);
                lSimpleDataTable = await _msSqlProvider
                    .StoredProcAsync(_strUpdateCommand, lSqlParameters, lSimpleDataTable,
                        LoadModel);
            }
            catch (Exception lException)
            {
                _logger.LogError("Update error.", lException);
                return null;
            }

            return lSimpleDataTable;
        }

        public ISqlQueueItem GetDeleteQueueItem(TType model)
        {
            return new SqlQueueItem(DeleteModel(model), _strDeleteCommand);
        }

        public ISqlQueueItem GetSaveQueueItem(TType model)
        {
            return new SqlQueueItem(SaveModel(model), _strSaveCommand);
        }

        public ISqlQueueItem GetUpdateQueueItem(TType model)
        {
            return new SqlQueueItem(UpdateModel(model), _strUpdateCommand);
        }

        protected abstract List<ISqlParameter> DeleteModel(TType model);

        protected abstract List<ISqlParameter> SaveModel(TType model);

        protected abstract List<ISqlParameter> UpdateModel(TType model);

        public abstract TType LoadModel(IDataReader sqlDataReader);
    }
}