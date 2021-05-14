using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using DbProviderWrapper.Builders;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Models.Interfaces;
using DbProviderWrapper.Persistence;
using DbProviderWrapper.QueueExecution;
using Microsoft.Extensions.Logging;

namespace DbProviderWrapper
{
    public class DbProvider : IDbProvider, IDbQueueProvider
    {
        #region Fields

        private readonly IDbProviderFactory _providerFactory;
        private readonly ILogger _logger;
        private readonly IConnectionStringProvider _connectionStringProvider;

        #endregion

        #region Constructors

        public DbProvider(ILogger logger, IConnectionStringProvider connectionStringProvider,
            IDbProviderFactory factory)
        {
            _logger = logger;
            _connectionStringProvider = connectionStringProvider;
            _providerFactory = factory;
        }

        #endregion

        public ILogger Logger
        {
            get { return _logger; }
        }

        public List<IDictionary<string, object>> Select(string query, IEnumerable<string> columns)
        {
            try
            {
                DbConnection lConnection = _providerFactory.CreateConnection(_connectionStringProvider);
                lConnection.Open();

                DbTransaction lTransaction = lConnection.BeginTransaction();

                DbCommand lCommand =
                    _providerFactory.CreateCommand(lConnection, lTransaction, query, CommandType.Text, null);

                List<IDictionary<string, object>> lData = new List<IDictionary<string, object>>();

                try
                {
                    IDataReader lSqlDataReader = lCommand.ExecuteReader();
                    try
                    {
                        IEnumerable<string> lEnumerable = columns as string[] ?? columns.ToArray();
                        while (lSqlDataReader.Read())
                        {
                            Dictionary<string, object> lRow =
                                lEnumerable.ToDictionary(column => column, column => lSqlDataReader[column]);

                            lData.Add(lRow);
                        }

                        lSqlDataReader.Close();
                        lSqlDataReader.Dispose();

                        lTransaction.Commit();
                    }
                    catch (Exception lException)
                    {
                        _logger.LogError($"{_providerFactory.DatabaseType} command execution fail {query}",
                            lException);

                        lTransaction.Rollback();
                    }
                    finally
                    {
                        lSqlDataReader.Close();
                        lSqlDataReader.Dispose();
                    }
                }
                catch (Exception lException)
                {
                    _logger.LogError($"{_providerFactory.DatabaseType}  command execution error {query}",
                        lException);

                    lTransaction.Rollback();
                }

                DisposeCommand(lCommand, lTransaction, lConnection);

                return lData;
            }
            catch (Exception lException)
            {
                _logger.LogError("DbProvider.Select", lException);
            }

            return null;
        }

        public object SimpleSelect(string query)
        {
            try
            {
                DbConnection lConnection = _providerFactory.CreateConnection(_connectionStringProvider);
                lConnection.Open();

                DbTransaction lTransaction = lConnection.BeginTransaction();

                DbCommand lCommand =
                    _providerFactory.CreateCommand(lConnection, lTransaction, query, CommandType.Text, null);

                try
                {
                    IDataReader lSqlDataReader = lCommand.ExecuteReader();

                    try
                    {
                        while (lSqlDataReader.Read())
                        {
                            object lResult = lSqlDataReader[0];

                            lSqlDataReader.Close();
                            lSqlDataReader.Dispose();
                            lTransaction.Commit();

                            return lResult;
                        }
                    }
                    catch (Exception lException)
                    {
                        _logger.LogError($"{_providerFactory.DatabaseType}  command execution fail {query}",
                            lException);
                    }
                    finally
                    {
                        lSqlDataReader.Close();
                        lSqlDataReader.Dispose();
                    }

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.LogError($"{_providerFactory.DatabaseType}  command execution error {query}",
                        lException);

                    lTransaction.Rollback();
                }
                finally
                {
                    DisposeCommand(lCommand, lTransaction, lConnection);
                }

                return null;
            }
            catch (Exception lException)
            {
                _logger.LogError("DbProvider.SimpleSelect", lException);
            }

            return null;
        }

        public IDataReader StoredProc(string procedureName, IEnumerable<ISqlParameter> sqlParameters)
        {
            try
            {
                DbConnection lConnection = _providerFactory.CreateConnection(_connectionStringProvider);
                lConnection.Open();

                DbTransaction lTransaction = lConnection.BeginTransaction();

                DbCommand lSqlCommand = null;
                sqlParameters = sqlParameters?.ToList();
                try
                {
                    lSqlCommand = _providerFactory.CreateCommand(lConnection, lTransaction, procedureName,
                        CommandType.StoredProcedure, sqlParameters);

                    lSqlCommand.ExecuteNonQuery();

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.LogError(
                        $"{_providerFactory.DatabaseType}  command execution error {procedureName}",
                        lException);
                    _logger.LogError(
                        $"Parameters:\n{string.Join("\n", sqlParameters?.Select(x => x.Name + "=" + x.Value).ToArray() ?? Array.Empty<string>())}");
                    lTransaction.Rollback();
                }
                finally
                {
                    DisposeCommand(lSqlCommand, lTransaction, lConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.LogError("DbProvider.StoredProc:" + procedureName, lException);
            }

            return null;
        }

        public async Task<IDataReader> StoredProcAsync(string procedureName, IEnumerable<ISqlParameter> sqlParameters)
        {
            try
            {
                DbConnection lConnection = _providerFactory.CreateConnection(_connectionStringProvider);
                await lConnection.OpenAsync();

                DbTransaction lTransaction = await lConnection.BeginTransactionAsync();

                DbCommand lSqlCommand = null;
                sqlParameters = sqlParameters?.ToList();
                try
                {
                    lSqlCommand = _providerFactory.CreateCommand(lConnection, lTransaction, procedureName,
                        CommandType.StoredProcedure, sqlParameters);

                    await lSqlCommand.ExecuteNonQueryAsync();

                    await lTransaction.CommitAsync();
                }
                catch (Exception lException)
                {
                    _logger.LogError(
                        $"{_providerFactory.DatabaseType}  command execution error {procedureName}",
                        lException);
                    _logger.LogError(
                        $"Parameters:\n{string.Join("\n", (sqlParameters ?? Array.Empty<ISqlParameter>()).Select(x => x.Name + "=" + x.Value).ToArray())}");
                    await lTransaction.RollbackAsync();
                }
                finally
                {
                    await DisposeCommandAsync(lSqlCommand, lTransaction, lConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.LogError("DbProvider.StoredProc:" + procedureName, lException);
            }

            return null;
        }

        public void StoredProc<TType>(string procedureName, IEnumerable<ISqlParameter> sqlParameters,
            ref SimpleDataTable<TType> simpleDataTable,
            Func<IDataReader, TType> loadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            try
            {
                DbConnection lConnection = _providerFactory.CreateConnection(_connectionStringProvider);
                lConnection.Open();

                DbTransaction lTransaction = lConnection.BeginTransaction();

                DbCommand lSqlCommand = null;
                sqlParameters = sqlParameters?.ToList();

                try
                {
                    lSqlCommand = _providerFactory.CreateCommand(lConnection, lTransaction, procedureName,
                        CommandType.StoredProcedure, sqlParameters);

                    IDataReader lSqlDataReader = lSqlCommand.ExecuteReader();

                    while (lSqlDataReader.Read())
                        simpleDataTable.AddNewRow(loadModel(lSqlDataReader));

                    lSqlDataReader.Close();
                    lSqlDataReader.Dispose();

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.LogError(
                        $"{_providerFactory.DatabaseType}  command execution error {procedureName}",
                        lException);
                    _logger.LogError(
                        $"Parameters:\n{string.Join("\n", (sqlParameters ?? Array.Empty<ISqlParameter>()).Select(x => x.Name + "=" + x.Value).ToArray())}");
                }
                finally
                {
                    DisposeCommand(lSqlCommand, lTransaction, lConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.LogError("DbProvider.StoredProc<TType>:" + procedureName, lException);
            }
        }

        public async Task<SimpleDataTable<TType>> StoredProcAsync<TType>(string procedureName,
            IEnumerable<ISqlParameter> sqlParameters, SimpleDataTable<TType> simpleDataTable,
            Func<IDataReader, TType> loadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            try
            {
                DbConnection lConnection = _providerFactory.CreateConnection(_connectionStringProvider);
                await lConnection.OpenAsync();

                DbTransaction lTransaction = await lConnection.BeginTransactionAsync();

                DbCommand lSqlCommand = null;
                sqlParameters = sqlParameters?.ToList();

                try
                {
                    lSqlCommand = _providerFactory.CreateCommand(lConnection, lTransaction, procedureName,
                        CommandType.StoredProcedure, sqlParameters);

                    DbDataReader lSqlDataReader = await lSqlCommand.ExecuteReaderAsync();

                    while (await lSqlDataReader.ReadAsync())
                        simpleDataTable.AddNewRow(loadModel(lSqlDataReader));

                    await lSqlDataReader.CloseAsync();
                    await lSqlDataReader.DisposeAsync();

                    await lTransaction.CommitAsync();
                }
                catch (Exception lException)
                {
                    _logger.LogError(
                        $"{_providerFactory.DatabaseType}  command execution error {procedureName}",
                        lException);
                    _logger.LogError(
                        $"Parameters:\n{string.Join("\n", (sqlParameters ?? Array.Empty<ISqlParameter>()).Select(x => x.Name + "=" + x.Value).ToArray())}");
                }
                finally
                {
                    await DisposeCommandAsync(lSqlCommand, lTransaction, lConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.LogError("DbProvider.StoredProc<TTYPE>:" + procedureName, lException);
            }

            return simpleDataTable;
        }

        public DbConnection CreateConnection()
        {
            return _providerFactory.CreateConnection(_connectionStringProvider);
        }

        public async Task<bool> StoredProcAsync(string procedureName, IEnumerable<ISqlParameter> parameters,
            DbConnection connection, DbTransaction transaction)
        {
            bool lRes = true;
            try
            {
                DbCommand lSqlCommand = null;
                parameters = parameters?.ToList();

                try
                {
                    lSqlCommand = _providerFactory.CreateCommand(connection, transaction, procedureName,
                        CommandType.StoredProcedure, parameters);

                    await lSqlCommand.ExecuteNonQueryAsync();
                }
                catch (Exception lException)
                {
                    _logger.LogError(
                        $"{_providerFactory.DatabaseType}  command execution error {procedureName}",
                        lException);
                    _logger.LogError(
                        $"Parameters:\n{string.Join("\n", (parameters ?? Array.Empty<ISqlParameter>()).Select(x => x.Name + "=" + x.Value).ToArray())}");
                    await transaction.RollbackAsync();
                }
                finally
                {
                    await DisposeCommandAsync(lSqlCommand);
                }
            }
            catch (Exception lException)
            {
                _logger.LogError("MsSqlProvider.StoredProc:" + procedureName, lException);
                lRes = false;
            }

            return lRes;
        }

        public async Task DisposeConnectionAsync(DbTransaction transaction, DbConnection sqlConnection)
        {
            try
            {
                await transaction.DisposeAsync();
            }
            catch
            {
                // ignored
            }

            try
            {
                await sqlConnection.CloseAsync();
                await sqlConnection.DisposeAsync();
            }
            catch
            {
                // ignored
            }
        }

        private static async Task DisposeCommandAsync(DbCommand sqlCommand, DbTransaction transaction,
            DbConnection sqlConnection)
        {
            if (sqlCommand != null)
            {
                sqlCommand.Parameters.Clear();
                await sqlCommand.DisposeAsync();
            }

            try
            {
                await transaction.DisposeAsync();
            }
            catch
            {
                // ignored
            }

            try
            {
                await sqlConnection.CloseAsync();
                await sqlConnection.DisposeAsync();
            }
            catch
            {
                // ignored
            }
        }

        private static void DisposeCommand(IDbCommand sqlCommand, IDbTransaction transaction,
            IDbConnection sqlConnection)
        {
            if (sqlCommand != null)
            {
                sqlCommand.Parameters.Clear();
                sqlCommand.Dispose();
            }

            try
            {
                transaction.Dispose();
            }
            catch
            {
                // ignored
            }

            try
            {
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        private async Task DisposeCommandAsync(DbCommand sqlCommand)
        {
            if (sqlCommand != null)
            {
                sqlCommand.Parameters.Clear();
                await sqlCommand.DisposeAsync();
            }
        }
    }
}