using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DbProviderWrapper.Helpers;

namespace DbProviderWrapper.AbstractExecutor.Mapping
{
    public class PropertiesMapper<TEntity> : IPropertiesMapping
    {
        private readonly TEntity _entity;
        #region Fields

        private readonly Dictionary<string, PropertyInfo> _propertyInfos = new Dictionary<string, PropertyInfo>();

        public PropertiesMapper(TEntity entity)
        {
            _entity = entity;
        }

        #endregion

        public IDictionary<string, PropertyInfo> GetProperties()
        {
            return _propertyInfos;
        }

        public void MapProperty<TProperty>(string dbColumnName,
            Expression<Func<TEntity, TProperty>> property)
        {
            _propertyInfos.Add(dbColumnName, _entity.GetPropertyInfo(property));
        }
    }
}