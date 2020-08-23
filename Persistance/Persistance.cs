#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DbProviderWrapper.MsSql;

#endregion

namespace DbProviderWrapper.Persistance
{
    public abstract class Persistance<TTYPE> : IMsSqlPersistance<TTYPE>
    {
        #region Fields

        private readonly string _strDeleteCommand;
        private readonly string _strLoadCommand;
        private readonly string _strSaveCommand;
        private readonly string _strUpdateCommand;
        private readonly string _strImportCommand;

        private readonly IMsSqlProvider _msSqlProvider;

        #endregion

        #region Constructors

        protected Persistance(
            string strLoadCommand,
            string strSaveCommand,
            string strUpdateCommand,
            string strDeleteCommand,
            string strImportCommand,
            IMsSqlProvider msSqlProvider)
        {
            _strLoadCommand = strLoadCommand;
            _strSaveCommand = strSaveCommand;
            _strUpdateCommand = strUpdateCommand;
            _strDeleteCommand = strDeleteCommand;
            _strImportCommand = strImportCommand;
            _msSqlProvider = msSqlProvider;
        }

        #endregion

        public virtual bool Delete(TTYPE model)
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

        public virtual async Task<bool> DeleteAsync(TTYPE model)
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

        public virtual SimpleDataTable<TTYPE> Load(List<SqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            SimpleDataTable<TTYPE> lSimpleDataTable = new SimpleDataTable<TTYPE>();

            _msSqlProvider
                .StoredProc(_strLoadCommand, parameters ?? new List<SqlParameter>(), ref lSimpleDataTable, LoadModel,
                    isolationLevel);

            return lSimpleDataTable;
        }

        public virtual async Task<SimpleDataTable<TTYPE>> LoadAsync(List<SqlParameter> parameters = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            SimpleDataTable<TTYPE> lSimpleDataTable = new SimpleDataTable<TTYPE>();

            lSimpleDataTable = await _msSqlProvider
                .StoredProcAsync(_strLoadCommand, parameters ?? new List<SqlParameter>(), lSimpleDataTable, LoadModel,
                    isolationLevel);

            return lSimpleDataTable;
        }

        public virtual SimpleDataTable<TTYPE> Save(TTYPE model)
        {
            SimpleDataTable<TTYPE> lSimpleDataTable = new SimpleDataTable<TTYPE>();
            try
            {
                TTYPE lInstance = Activator.CreateInstance<TTYPE>();
                lSimpleDataTable.BindColumns(SaveModel(lInstance));
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

        public virtual async Task<SimpleDataTable<TTYPE>> SaveAsync(TTYPE model)
        {
            SimpleDataTable<TTYPE> lSimpleDataTable = new SimpleDataTable<TTYPE>();
            try
            {
                TTYPE lInstance = Activator.CreateInstance<TTYPE>();
                lSimpleDataTable.BindColumns(SaveModel(lInstance));
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

        public virtual SimpleDataTable<TTYPE> Update(TTYPE model)
        {
            SimpleDataTable<TTYPE> lSimpleDataTable = new SimpleDataTable<TTYPE>();
            try
            {
                TTYPE lInstance = Activator.CreateInstance<TTYPE>();
                lSimpleDataTable.BindColumns(UpdateModel(lInstance));
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

        public virtual async Task<SimpleDataTable<TTYPE>> UpdateAsync(TTYPE model)
        {
            SimpleDataTable<TTYPE> lSimpleDataTable = new SimpleDataTable<TTYPE>();
            try
            {
                TTYPE lInstance = Activator.CreateInstance<TTYPE>();
                lSimpleDataTable.BindColumns(UpdateModel(lInstance));
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

        public SimpleDataTable<TTYPE> ImportTable(DataTable dataTable)
        {
            SimpleDataTable<TTYPE> lSimpleDataTable = new SimpleDataTable<TTYPE>();
            try
            {
                TTYPE lInstance = Activator.CreateInstance<TTYPE>();
                lSimpleDataTable.BindColumns(UpdateModel(lInstance));
                List<SqlParameter> lSqlParameters = ImportModels(dataTable);
                _msSqlProvider
                    .StoredProc(_strUpdateCommand, lSqlParameters ?? new List<SqlParameter>(), ref lSimpleDataTable,
                        LoadModel);
            }
            catch (Exception lException)
            {
                _msSqlProvider.Logger.WriteExceptionLog("Import error.", lException);
                return null;
            }

            return lSimpleDataTable;
        }

        public async Task<SimpleDataTable<TTYPE>> ImportTableAsync(DataTable dataTable)
        {
            SimpleDataTable<TTYPE> lSimpleDataTable = new SimpleDataTable<TTYPE>();
            try
            {
                TTYPE lInstance = Activator.CreateInstance<TTYPE>();
                lSimpleDataTable.BindColumns(UpdateModel(lInstance));
                List<SqlParameter> lSqlParameters = ImportModels(dataTable);
                lSimpleDataTable = await _msSqlProvider
                    .StoredProcAsync(_strImportCommand, lSqlParameters ?? new List<SqlParameter>(), lSimpleDataTable,
                        LoadModel);
            }
            catch (Exception lException)
            {
                _msSqlProvider.Logger.WriteExceptionLog("Import error.", lException);
                return null;
            }

            return lSimpleDataTable;
        }

        protected abstract List<SqlParameter> DeleteModel(TTYPE model);

        protected TTYPE LoadModel(SqlDataReader sqlDataReader)
        {
            return Load(sqlDataReader);
        }

        protected abstract List<SqlParameter> SaveModel(TTYPE model);

        protected abstract List<SqlParameter> UpdateModel(TTYPE model);
        protected abstract List<SqlParameter> ImportModels(DataTable table);

        public abstract TTYPE Load(SqlDataReader sqlDataReader);
    }
}