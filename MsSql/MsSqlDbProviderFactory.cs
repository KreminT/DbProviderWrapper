using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using DbProviderWrapper.Builders;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.MsSql
{
    public class MsSqlDbProviderFactory : IDbProviderFactory
    {
        #region Fields

        private readonly IParameterFactory<SqlParameter> _parameterFactory = new MsSqlParameterFactory();
        private readonly string _databaseType="MS SQL";

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
            return new SqlConnection(connectionStringProvider.GetConnectionString());
        }

        public DbCommand CreateCommand(DbConnection connection, DbTransaction transaction, string commandText,
            CommandType commandType, IEnumerable<ISqlParameter> parameters)
        {
            DbCommand lCommand = new SqlCommand();
            lCommand.Connection = connection;
            lCommand.Transaction = transaction;
            lCommand.CommandText = commandText;
            lCommand.CommandType = commandType;
            foreach (SqlParameter lSqlParameter in parameters.GetParameters(_parameterFactory))
                lCommand.Parameters.Add(lSqlParameter);
            return lCommand;
        }
    }
}