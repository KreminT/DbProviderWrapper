using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DbProviderWrapper.Builders;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Models.Interfaces;
using MySql.Data.MySqlClient;

namespace DbProviderWrapper.MySql
{
    public class MySqlDbProviderFactory : IDbProviderFactory
    {
        #region Fields

        private readonly IParameterFactory<MySqlParameter> _parameterFactory = new MySqlParameterFactory();
        private readonly string _databaseType = "MySQL";

        #endregion

        public string DatabaseType
        {
            get { return _databaseType; }
        }

        public IParameterFactory<TType> CreateParameterFactory<TType>()
        {
            return (IParameterFactory<TType>) _parameterFactory;
        }

        public DbConnection CreateConnection(IConnectionStringProvider connectionStringProvider)
        {
            return new MySqlConnection(connectionStringProvider.GetConnectionString());
        }

        public DbCommand CreateCommand(DbConnection connection, DbTransaction transaction, string commandText,
            CommandType commandType, IEnumerable<ISqlParameter> parameters)
        {
            DbCommand lCommand = new MySqlCommand();
            lCommand.Connection = connection;
            lCommand.Transaction = transaction;
            lCommand.CommandText = commandText;
            lCommand.CommandType = commandType;
            foreach (MySqlParameter lSqlParameter in parameters.GetParameters(_parameterFactory))
                lCommand.Parameters.Add(lSqlParameter);
            return lCommand;
        }
    }
}