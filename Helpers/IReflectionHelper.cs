using System.Collections.Generic;
using System.Reflection;

namespace DbProviderWrapper.Helpers
{
    public interface IReflectionHelper
    {
        IDictionary<string, PropertyInfo> GetProperties();
    }
}