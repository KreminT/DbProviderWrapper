using System.Collections.Generic;
using System.Reflection;

namespace DbProviderWrapper.Mapping
{
    public interface IPropertiesMapping
    {
        IDictionary<string, PropertyInfo> GetProperties();
    }
}