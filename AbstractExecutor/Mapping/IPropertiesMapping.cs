using System.Collections.Generic;
using System.Reflection;

namespace DbProviderWrapper.AbstractExecutor.Mapping
{
    public interface IPropertiesMapping
    {
        IDictionary<string, PropertyInfo> GetProperties();
    }
}