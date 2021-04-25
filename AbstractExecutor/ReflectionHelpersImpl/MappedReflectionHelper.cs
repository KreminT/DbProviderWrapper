using System.Collections.Generic;
using System.Reflection;
using DbProviderWrapper.AbstractExecutor.Mapping;

namespace DbProviderWrapper.AbstractExecutor.ReflectionHelpersImpl
{
    public class MappedReflectionHelper : IReflectionHelper
    {
        #region Fields

        private readonly IPropertiesMapping _mapping;

        #endregion

        #region Constructors

        public MappedReflectionHelper(IPropertiesMapping mapping)
        {
            _mapping = mapping;
        }

        #endregion

        public IDictionary<string, PropertyInfo> GetProperties<TType>(TType obj)
        {
            return _mapping.GetProperties();
        }
    }
}