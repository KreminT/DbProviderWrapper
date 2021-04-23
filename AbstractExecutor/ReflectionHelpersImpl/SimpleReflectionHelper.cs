using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DbProviderWrapper.AbstractExecutor.ReflectionHelpersImpl
{
    public class SimpleReflectionHelper : IReflectionHelper
    {
        public IDictionary<string, PropertyInfo> GetProperties<TType>(TType obj)
        {
            Type lArgsType = typeof(TType);

            return lArgsType.GetProperties().ToDictionary(propertyInfo => propertyInfo.Name);
        }
    }
}