using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Models.Interfaces;
using DbProviderWrapper.Persistence;

namespace DbProviderWrapper.AbstractExecutor
{
    public class AbstractExecutor : IAbstractExecutor
    {
        #region Fields

        private readonly IDbProvider _provider;
        private readonly ISqlParametersGenerator _sqlParameters;
        private readonly IReflectionHelper _reflectionHelper;
        private readonly IDbLogger _logger;

        #endregion

        #region Constructors

        public AbstractExecutor(IDbProvider provider, ISqlParametersGenerator sqlParameters,
            IReflectionHelper reflectionHelper, IDbLogger logger)
        {
            _provider = provider;
            _sqlParameters = sqlParameters;
            _reflectionHelper = reflectionHelper;
            _logger = logger;
        }

        #endregion

        public async Task<SimpleDataTable<TTypeResult>> Execute<TTypeResult, TTypeArgs>(string commandName,
            TTypeArgs args)
        {
            SimpleDataTable<TTypeResult> lSimpleDataTable = new SimpleDataTable<TTypeResult>();
            try
            {
                List<ISqlParameter> lSqlParameters = _sqlParameters.BuildSqlParameters(args);
                lSimpleDataTable =
                    await _provider.StoredProcAsync(commandName, lSqlParameters, lSimpleDataTable, Load<TTypeResult>);
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("AbstractExecutor error.", lException);
                return null;
            }

            return lSimpleDataTable;
        }

        protected virtual TType Load<TType>(IDataReader dataReader)
        {
            TType lInstance = Activator.CreateInstance<TType>();
            foreach ((string lKey, PropertyInfo lPropertyInfo) in _reflectionHelper.GetProperties<TType>(lInstance))
                if (DbHelper.ColumnExists(dataReader, lKey))
                {
                    object lValue = dataReader[lKey];
                    lPropertyInfo.SetValue(lInstance, lValue);
                }

            return lInstance;
        }
    }
}