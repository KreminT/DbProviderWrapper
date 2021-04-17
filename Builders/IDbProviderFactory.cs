using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.Builders
{
    public interface IDbProviderFactory
    {
        string DatabaseType { get; }
        IParameterFactory<TType> CreateParameterFactory<TType>();
        DbConnection CreateConnection(IConnectionStringProvider connectionStringProvider);

        DbCommand CreateCommand(DbConnection connection, DbTransaction transaction, string commandText,
            CommandType commandType, IEnumerable<ISqlParameter> parameters);
    }
}