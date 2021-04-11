#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DbProviderWrapper.Interfaces;
using DbProviderWrapper.MsSql;

#endregion

namespace DbProviderWrapper.Persistence
{
    public abstract class Persistence<TType> : IMsSqlPersistence<TType>
    {
        #region Fields

        private readonly string _strDeleteCommand;
        private readonly string _strLoadCommand;
        private readonly string _strSaveCommand;
        private readonly string _strUpdateCommand;

        private readonly IMsSqlProvider _msSqlProvider;

        #endregion

        #region Constructors

        protected Persistence(
            string strLoadCommand,
            string strSaveCommand,
            string strUpdateCommand,
            string strDeleteCommand,
            IMsSqlProvider msSqlProvider)
        {
            _strLoadCommand = strLoadCommand;
            _strSaveCommand = strSaveCommand;
            _strUpdateCommand = strUpdateCommand;
            _strDeleteCommand = strDeleteCommand;
            _msSqlProvider = msSqlProvider;
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
                _msSqlProvider.Logger.WriteExceptionLog("Delete error.", lException);
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
                _msSqlProvider.Logger.WriteExceptionLog("Delete error.", lException);
            }

            return false;
        }

        public virtual SimpleDataTable<TType> Load(List<SqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            SimpleDataTable<TType> lSimpleDataTable = new SimpleDataTable<TType>();

            _msSqlProvider
                .StoredProc(_strLoadCommand, parameters ?? new List<SqlParameter>(), ref lSimpleDataTable, LoadModel,
                    isolationLevel);

            return lSimpleDataTable;
        }

        public virtual async Task<SimpleDataTable<TType>> LoadAsync(List<SqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            SimpleDataTable<TType> lSimpleDataTable = new SimpleDataTable<TType>();

            lSimpleDataTable = await _msSqlProvider
                .StoredProcAsync(_strLoadCommand, parameters ?? new List<SqlParameter>(), lSimpleDataTable, LoadModel,
                    isolationLevel);

            return lSimpleDataTable;
        }

        public virtual SimpleDataTable<TType> Save(TType model)
        {
            SimpleDataTable<TType> lSimpleDataTable = new SimpleDataTable<TType>();
            try
            {
                TType lInstance = Activator.CreateInstance<TType>();
                List<SqlParameter> lSqlParameters = SaveModel(model);
                _msSqlProvider
                    .StoredProc(_strSaveCommand, lSqlParameters ?? new List<SqlParameter>(), ref lSimpleDataTable,
                        LoadModel);
            }
            catch (Exception lException)
            {
                _msSqlProvider.Logger.WriteExceptionLog("Save error.", lException);
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
                List<SqlParameter> lSqlParameters = SaveModel(model);
                lSimpleDataTable = await _msSqlProvider
                    .StoredProcAsync(_strSaveCommand, lSqlParameters ?? new List<SqlParameter>(), lSimpleDataTable,
                        LoadModel);
            }
            catch (Exception lException)
            {
                _msSqlProvider.Logger.WriteExceptionLog("Save error.", lException);
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
                List<SqlParameter> lSqlParameters = UpdateModel(model);
                _msSqlProvider
                    .StoredProc(_strUpdateCommand, lSqlParameters ?? new List<SqlParameter>(), ref lSimpleDataTable,
                        LoadModel);
            }
            catch (Exception lException)
            {
                _msSqlProvider.Logger.WriteExceptionLog("Update error.", lException);
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
                List<SqlParameter> lSqlParameters = UpdateModel(model);
                lSimpleDataTable = await _msSqlProvider
                    .StoredProcAsync(_strUpdateCommand, lSqlParameters ?? new List<SqlParameter>(), lSimpleDataTable,
                        LoadModel);
            }
            catch (Exception lException)
            {
                _msSqlProvider.Logger.WriteExceptionLog("Update error.", lException);
                return null;
            }

            return lSimpleDataTable;
        }

        protected abstract List<SqlParameter> DeleteModel(TType model);

        protected TType LoadModel(SqlDataReader sqlDataReader)
        {
            return Load(sqlDataReader);
        }

        protected abstract List<SqlParameter> SaveModel(TType model);

        protected abstract List<SqlParameter> UpdateModel(TType model);

        public abstract TType Load(SqlDataReader sqlDataReader);
    }
}