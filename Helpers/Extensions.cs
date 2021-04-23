﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DbProviderWrapper.Builders;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.Helpers
{
    internal static class Extensions
    {
        internal static List<TParameterType> GetParameters<TParameterType>(
            this IEnumerable<ISqlParameter> parameters, IParameterFactory<TParameterType> factory)
        {
            return parameters == null
                ? new List<TParameterType>()
                : parameters.Select(item => item.GetParameter(factory)).ToList();
        }

        internal static PropertyInfo GetPropertyInfo<TSource, TProperty>(
            this TSource source,
            Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type lType = typeof(TSource);

            MemberExpression lMember = propertyLambda.Body as MemberExpression;
            if (lMember == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");

            PropertyInfo lPropInfo = lMember.Member as PropertyInfo;
            if (lPropInfo == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");

            if (lType != lPropInfo.ReflectedType &&
                !lType.IsSubclassOf(lPropInfo.ReflectedType!))
                throw new ArgumentException(
                    $"Expression '{propertyLambda}' refers to a property that is not from type {lType}.");

            return lPropInfo;
        }
    }
}