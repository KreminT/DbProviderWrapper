using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Models.Interfaces;
using DbProviderWrapper.Persistence;
using Microsoft.Extensions.Logging;

namespace DbProviderWrapper.AbstractExecutor
{
    public class AbstractExecutor : IAbstractExecutor
    {
        #region Constructors

        public AbstractExecutor(IDbProvider provider, ILogger logger)
        {
            _provider = provider;
            _logger = logger;
        }

        #endregion

        public async Task<SimpleDataTable<TTypeResult>> Execute<TTypeResult, TTypeArgs>(string commandName,
            TTypeArgs args, ISqlParametersBuilder sqlParameters, IReflectionHelper reflectionHelper)
        {
            SimpleDataTable<TTypeResult> lSimpleDataTable = new SimpleDataTable<TTypeResult>();
            try
            {
                List<ISqlParameter> lSqlParameters = sqlParameters.BuildSqlParameters(args);
                lSimpleDataTable =
                    await _provider.StoredProcAsync(commandName, lSqlParameters, lSimpleDataTable,
                        new Loader(reflectionHelper).Load<TTypeResult>);
            }
            catch (Exception lException)
            {
                _logger.LogError("AbstractExecutor error.", lException);
                return null;
            }

            return lSimpleDataTable;
        }

        private class Loader
        {
            private readonly IReflectionHelper _reflectionHelper;

            public Loader(IReflectionHelper reflectionHelper)
            {
                _reflectionHelper = reflectionHelper;
            }

            public TType Load<TType>(IDataReader dataReader)
            {
                TType lInstance = Activator.CreateInstance<TType>();
                foreach ((string lKey, PropertyInfo lPropertyInfo) in _reflectionHelper.GetProperties(lInstance))
                    if (DbHelper.ColumnExists(dataReader, lKey))
                    {
                        object lValue = dataReader[lKey];
                        lPropertyInfo.SetValue(lInstance, lValue);
                    }

                return lInstance;
            }
        }

        #region Fields

        private readonly IDbProvider _provider;
        private readonly ILogger _logger;

        #endregion
    }
}