using System;
using System.Collections.Generic;
using DbProviderWrapper.Builders;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.MsSql;
using DbProviderWrapper.MySql;
using Microsoft.Extensions.DependencyInjection;

namespace DbProviderWrapper
{
    public static class IoTHelper
    {
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
                    DbProvider lProvider = new DbProvider(serviceProvider.GetService<IDbLogger>(),
                        serviceProvider.GetService<ConnectionStringProviderResolver>()?.Invoke(key),
                        serviceProvider.GetService<DbProviderFactoryResolver>()?.Invoke(key));
                    _providers[key] = lProvider;
                }

                return _providers[key];
            });
        }

        private delegate IDbProviderFactory DbProviderFactoryResolver(string key);
    }
}