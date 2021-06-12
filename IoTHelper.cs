using System;
using System.Collections.Generic;
using DbProviderWrapper.AbstractExecutor;
using DbProviderWrapper.Builders;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.MsSql;
using DbProviderWrapper.MySql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbProviderWrapper
{
    public static class IoTHelper
    {
        public delegate IAbstractExecutor AbstractExecutorResolver(string key);

        public delegate IConnectionStringProvider ConnectionStringProviderResolver(string key);

        public delegate IDbProvider DbProviderResolver(string key);

        public const string MSSQL = "MSSQL";
        public const string MYSQL = "MYSQL";

        private static Dictionary<string, DbProvider> _providers;

        public static void DbProviderRegister(this IServiceCollection services,
            ConnectionStringProviderResolver connectionStringProviderResolver)
        {
            services.AddSingleton<MySqlDbProviderFactory>();
            services.AddSingleton<MsSqlDbProviderFactory>();

            services.AddSingleton(connectionStringProviderResolver);
            services.AddSingleton<DbProviderFactoryResolver>(serviceProvider => key =>
            {
                switch (key)
                {
                    case MSSQL:
                        return serviceProvider.GetService<MsSqlDbProviderFactory>();
                    case MYSQL:
                        return serviceProvider.GetService<MySqlDbProviderFactory>();
                    default:
                        throw new ArgumentException("Unknown IDbProviderFactory");
                }
            });

            _providers = new Dictionary<string, DbProvider>();

            services.AddTransient<DbProviderResolver>(serviceProvider => key =>
            {
                if (!_providers.ContainsKey(key))
                {
                    DbProvider lProvider = new DbProvider(serviceProvider.GetService<ILogger>(),
                        serviceProvider.GetService<ConnectionStringProviderResolver>()?.Invoke(key),
                        serviceProvider.GetService<DbProviderFactoryResolver>()?.Invoke(key));
                    _providers[key] = lProvider;
                }

                return _providers[key];
            });
            services.AddTransient<AbstractExecutorResolver>(serviceProvider => key =>
            {
                DbProviderResolver lDbProviderResolver = serviceProvider.GetService<DbProviderResolver>();
                if (lDbProviderResolver != null)
                    return new AbstractExecutor.AbstractExecutor(lDbProviderResolver.Invoke(key),
                        serviceProvider.GetService<ILogger>());
                throw new ArgumentException("Unknown IAbstractExecutor");
            });
        }

        private delegate IDbProviderFactory DbProviderFactoryResolver(string key);
    }
}