using System.Collections.Generic;
using System.Linq;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Mapping;
using DbProviderWrapper.Models;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.AbstractExecutor.ParameterBuildersImpl
{
    public class MappedSqlParametersBuilder : ISqlParametersBuilder
    {
        #region Fields

        private readonly IPropertiesMapping _mapping;

        #endregion

        #region Constructors

        public MappedSqlParametersBuilder(IPropertiesMapping mapping)
        {
            _mapping = mapping;
        }

        #endregion

        public List<ISqlParameter> BuildSqlParameters<TTypeArgs>(TTypeArgs args)
        {
            return (from lPropertyInfo in _mapping.GetProperties()
                let lValue = lPropertyInfo.Value.GetValue(args)
                where !lValue.IsNullOrEmpty()
                select new SqlParameterWrapper(lPropertyInfo.Key, lValue)).Cast<ISqlParameter>().ToList();
        }
    }
}