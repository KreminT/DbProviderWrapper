using System;
using System.Collections.Generic;
using DbProviderWrapper.AbstractExecutor;
using DbProviderWrapper.Builders;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.MsSql;
using DbProviderWrapper.MySql;
using DbProviderWrapper.QueueExecution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbProviderWrapper
{
    public static class IoTHelper
    {
        public delegate IAbstractExecutor AbstractExecutorResolver(string key);

        public delegate IConnectionStringProvider ConnectionStringProviderResolver(string key);

        public delegate IDbProvider DbProviderResolver(string key);

        public delegate IDbQueueProvider DbQueueProviderResolver(string key);

        public const string MSSQL = "MSSQL";
        public const string MYSQL = "MYSQL";

        private static Dictionary<string, DbProvider> _providers;

        public static void DbProviderRegister(this IServiceCollection services,
            ConnectionStringProviderResolver connectionStringProviderResolver)
        {
            _providers = new Dictionary<string, DbProvider>();

            services.AddSingleton<MySqlDbProviderFactory>();
            services.AddSingleton<MsSqlDbProviderFactory>();

            services.AddSingleton(connectionStringProviderResolver);
            services.AddTransient<DbProviderFactoryResolver>(serviceProvider => key =>
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


            services.AddTransient<DbProviderResolver>(serviceProvider => key => GetProvider(serviceProvider, key));
            services.AddTransient<DbQueueProviderResolver>(serviceProvider => key => GetProvider(serviceProvider, key));

            services.AddTransient<AbstractExecutorResolver>(serviceProvider => key =>
            {
                DbProviderResolver lDbProviderResolver = serviceProvider.GetService<DbProviderResolver>();
                if (lDbProviderResolver != null)
                    return new AbstractExecutor.AbstractExecutor(lDbProviderResolver.Invoke(key),
                        serviceProvider.GetService<ILogger>());
                throw new ArgumentException("Unknown IAbstractExecutor");
            });
            services.Add(new ServiceDescriptor(typeof(ISqlQueueExecutor), typeof(QueueExecutor),
                ServiceLifetime.Singleton));
        }

        private static DbProvider GetProvider(IServiceProvider serviceProvider, string key)
        {
            if (_providers.ContainsKey(key)) return _providers[key];
            DbProvider lProvider = new DbProvider(serviceProvider.GetService<ILogger>(),
                serviceProvider.GetService<ConnectionStringProviderResolver>()?.Invoke(key),
                serviceProvider.GetService<DbProviderFactoryResolver>()?.Invoke(key));
            _providers[key] = lProvider;

            return _providers[key];
        }

        private delegate IDbProviderFactory DbProviderFactoryResolver(string key);
    }
}