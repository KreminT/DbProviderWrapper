using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DbProviderWrapper.Helpers;

namespace DbProviderWrapper.Mapping
{
    public class PropertiesMapper : IPropertiesMapping
    {
        #region Fields

        private readonly Dictionary<string, PropertyInfo> _propertyInfos = new Dictionary<string, PropertyInfo>();

        #endregion

        public IDictionary<string, PropertyInfo> GetProperties()
        {
            return _propertyInfos;
        }

        public void MapProperty<TEntity, TProperty>(TEntity entity, string dbColumnName,
            Expression<Func<TEntity, TProperty>> property)
        {
            _propertyInfos.Add(dbColumnName, entity.GetPropertyInfo(property));
        }
    }
}