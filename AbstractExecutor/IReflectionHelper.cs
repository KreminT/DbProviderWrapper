using System.Collections.Generic;
using System.Reflection;

namespace DbProviderWrapper.AbstractExecutor
{
    public interface IReflectionHelper
    {
        IDictionary<string, PropertyInfo> GetProperties<TType>(TType obj);
    }
}