using System.Collections.Generic;
using System.Linq;
using DbProviderWrapper.Interfaces;

namespace DbProviderWrapper.Helpers
{
    internal static class SqlParameterExtensions
    {
        internal static List<TParameterType> GetParameters<TParameterType>(
            this IEnumerable<ISqlParameter> parameters, IParameterFactory<TParameterType> factory)
        {
            return parameters == null
                ? new List<TParameterType>()
                : parameters.Select(item => item.GetParameter(factory)).ToList();
        }
    }
}