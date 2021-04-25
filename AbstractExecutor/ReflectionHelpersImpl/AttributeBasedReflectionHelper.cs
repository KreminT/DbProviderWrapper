using System;
using System.Collections.Generic;
using System.Reflection;
using DbProviderWrapper.AbstractExecutor.Attributes;

namespace DbProviderWrapper.AbstractExecutor.ReflectionHelpersImpl
{
    public class AttributeBasedReflectionHelper : IReflectionHelper
    {
        public IDictionary<string, PropertyInfo> GetProperties<TType>(TType obj)
        {
            Type lArgsType = typeof(TType);
            Dictionary<string, PropertyInfo> lInfo=new Dictionary<string, PropertyInfo>(); 
            foreach (PropertyInfo lPropertyInfo in lArgsType.GetProperties())
            {
                DbColumn lSqlArgumentAttribute = lPropertyInfo.GetCustomAttribute<DbColumn>();
                if (lSqlArgumentAttribute != null)
                {
                    string lPropertyName = lSqlArgumentAttribute.Name;
                    if (!string.IsNullOrEmpty(lPropertyName))
                    {
                        lInfo.Add(lPropertyName, lPropertyInfo);
                        continue;
                    }
                }
                lInfo.Add(lPropertyInfo.Name, lPropertyInfo);
            }

            return lInfo;
        }
    }
}