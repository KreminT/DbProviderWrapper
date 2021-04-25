using System;
using System.Collections.Generic;
using System.Linq;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Models;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.AbstractExecutor.ParameterBuildersImpl
{
    public class SimpleSqlParametersBuilder : ISqlParametersBuilder
    {
        public List<ISqlParameter> BuildSqlParameters<TTypeArgs>(TTypeArgs args)
        {
            Type lArgsType = typeof(TTypeArgs);
            List<ISqlParameter> lParameters = new List<ISqlParameter>();
            if (args == null) return lParameters;
            lParameters.AddRange((from lPropertyInfo in lArgsType.GetProperties()
                let lValue = lPropertyInfo.GetValue(args)
                where !lValue.IsNullOrEmpty()
                select new SqlParameterWrapper(lPropertyInfo.Name, lValue)));

            return lParameters;
        }
    }
}