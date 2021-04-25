using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbProviderWrapper.Attributes;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Models;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.AbstractExecutor.ParameterBuildersImpl
{
    public class AttributeBasedSqlParametersBuilder : ISqlParametersBuilder
    {
        public List<ISqlParameter> BuildSqlParameters<TTypeArgs>(TTypeArgs args)
        {
            Type lArgsType = typeof(TTypeArgs);
            List<ISqlParameter> lParameters = (from lPropertyInfo in lArgsType.GetProperties()
                let lSqlArgumentAttribute = lPropertyInfo.GetCustomAttribute<DbParameter>()
                where lSqlArgumentAttribute != null
                let lValue = lPropertyInfo.GetValue(args)
                where !lValue.IsNullOrEmpty()
                select new SqlParameterWrapper(lPropertyInfo.Name, lValue)).Cast<ISqlParameter>().ToList();

            return lParameters;
        }
    }
}