using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Interfaces;
using DbProviderWrapper.Persistence;
using MySql.Data.MySqlClient;

namespace DbProviderWrapper.MySql
{
    public class MySqlProvider : IDbProvider
    {
        #region Fields

        private readonly string _connectionString;
        private readonly IDbLogger _logger;
        private IParameterFactory<MySqlParameter> _parameterFactory;
        private IDbLogger _logger1;

        #endregion

        #region Constructors

        public MySqlProvider(IDbLogger logger, IConnectionStringProvider connectionStringProvider)
        {
            _logger = logger;
            _connectionString = connectionStringProvider.GetConnectionString();
            _parameterFactory = new MySqlParameterFactory();
        }

        #endregion

        public IDbLogger Logger
        {
            get { return _logger1; }
        }

        public List<IDictionary<string, object>> Select(string query, IEnumerable<string> columns)
        {
            try
            {
                MySqlConnection lSqlConnection = new MySqlConnection(_connectionString);
                lSqlConnection.Open();

                MySqlTransaction lTransaction = lSqlConnection.BeginTransaction();

                MySqlCommand lSqlCommand = new MySqlCommand(query)
                {
                    Connection = lSqlConnection, Transaction = lTransaction
                };

                List<IDictionary<string, object>> lData = new List<IDictionary<string, object>>();

                try
                {
                    MySqlDataReader lSqlDataReader = lSqlCommand.ExecuteReader();
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
                        _logger.WriteExceptionLog($"MySql command execution fail {query}",
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
                    _logger.WriteExceptionLog($"MsSql command execution error {query}", lException);

                    lTransaction.Rollback();
                }

                DisposeCommand(lSqlCommand, lTransaction, lSqlConnection);

                return lData;
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MsSqlProvider.Select", lException);
            }

            return null;
        }

        public object SimpleSelect(string query)
        {
            try
            {
                MySqlConnection lSqlConnection = new MySqlConnection(_connectionString);
                lSqlConnection.Open();

                MySqlTransaction lTransaction = lSqlConnection.BeginTransaction();

                MySqlCommand lSqlCommand = new MySqlCommand(query)
                {
                    Connection = lSqlConnection, Transaction = lTransaction
                };

                try
                {
                    MySqlDataReader lSqlDataReader = lSqlCommand.ExecuteReader();

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
                        _logger.WriteExceptionLog($"MySql command execution fail {query}",
                            lException);
                    }
                    finally
                    {
                        lSqlDataReader.Close();
                        lSqlDataReader.Dispose();
                        lSqlCommand = null;
                    }

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog($"MySql command execution fail {query}",
                        lException);

                    lTransaction.Rollback();
                }
                finally
                {
                    DisposeCommand(lSqlCommand, lTransaction, lSqlConnection);
                }

                return null;
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MySqlProvider.SimpleSelect", lException);
            }

            return null;
        }


        public IDataReader StoredProc(string procedureName, IEnumerable<ISqlParameter> sqlParameters)
        {
            try
            {
                MySqlConnection lSqlConnection = new MySqlConnection(_connectionString);
                lSqlConnection.Open();

                MySqlTransaction lTransaction = lSqlConnection.BeginTransaction();

                MySqlCommand lSqlCommand = null;
                IEnumerable<MySqlParameter> lSqlParameters = sqlParameters.GetParameters(_parameterFactory);
                try
                {
                    lSqlCommand = new MySqlCommand
                    {
                        Connection = lSqlConnection,
                        Transaction = lTransaction,
                        CommandText = procedureName,
                        CommandType = CommandType.StoredProcedure
                    };

                    foreach (MySqlParameter lSqlParameter in lSqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    lSqlCommand.ExecuteNonQuery();

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog(
                        $"MySql command execution error {procedureName}", lException);
                    _logger.WriteLog(
                        $"sqlcommandDelete:\n{string.Join("\n", lSqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())}");
                    lTransaction.Rollback();
                }
                finally
                {
                    DisposeCommand(lSqlCommand, lTransaction, lSqlConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MySqlProvider.StoredProc:" + procedureName, lException);
            }

            return null;
        }

        public async Task<IDataReader> StoredProcAsync(string procedureName, IEnumerable<ISqlParameter> sqlParameters)
        {
            try
            {
                MySqlConnection lSqlConnection = new MySqlConnection(_connectionString);
                await lSqlConnection.OpenAsync();

                MySqlTransaction lTransaction = await lSqlConnection.BeginTransactionAsync();

                MySqlCommand lSqlCommand = null;
                IEnumerable<MySqlParameter> lSqlParameters = sqlParameters.GetParameters(_parameterFactory);
                try
                {
                    lSqlCommand = new MySqlCommand
                    {
                        Connection = lSqlConnection,
                        Transaction = lTransaction,
                        CommandText = procedureName,
                        CommandType = CommandType.StoredProcedure
                    };


                    foreach (MySqlParameter lSqlParameter in lSqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    await lSqlCommand.ExecuteNonQueryAsync();

                    await lTransaction.CommitAsync();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog(
                        $"MySql command execution error {procedureName}", lException);
                    _logger.WriteLog(
                        $"sqlcommandDelete:\n{string.Join("\n", lSqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())}");
                    await lTransaction.RollbackAsync();
                }
                finally
                {
                    await DisposeCommandAsync(lSqlCommand, lTransaction, lSqlConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MySqlProvider.StoredProc:" + procedureName, lException);
            }

            return null;
        }

        public void StoredProc<TType>(
            string procedureName,
            IEnumerable<ISqlParameter> sqlParameters,
            ref SimpleDataTable<TType> simpleDataTable,
            Func<IDataReader, TType> loadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            try
            {
                MySqlConnection lSqlConnection = new MySqlConnection(_connectionString);
                lSqlConnection.Open();

                MySqlTransaction lTransaction = lSqlConnection.BeginTransaction(isolationLevel);

                MySqlCommand lSqlCommand = null;
                IEnumerable<MySqlParameter> lSqlParameters = sqlParameters.GetParameters(_parameterFactory);
                try
                {
                    lSqlCommand = new MySqlCommand
                    {
                        Connection = lSqlConnection,
                        Transaction = lTransaction,
                        CommandText = procedureName,
                        CommandType = CommandType.StoredProcedure
                    };


                    foreach (MySqlParameter lSqlParameter in lSqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    MySqlDataReader lSqlDataReader = lSqlCommand.ExecuteReader();

                    while (lSqlDataReader.Read())
                        simpleDataTable.AddNewRow(loadModel(lSqlDataReader));

                    lSqlDataReader.Close();
                    lSqlDataReader.Dispose();

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog(
                        $"MySql command execution error {procedureName}", lException);
                    _logger.WriteLog(
                        $"sqlcommand:\n{string.Join("\n", lSqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())}");
                }
                finally
                {
                    DisposeCommand(lSqlCommand, lTransaction, lSqlConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MySqlProvider.StoredProc<TTYPE>:" + procedureName, lException);
            }
        }

        public async Task<SimpleDataTable<TType>> StoredProcAsync<TType>(
            string procedureName,
            IEnumerable<ISqlParameter> sqlParameters,
            SimpleDataTable<TType> simpleDataTable,
            Func<IDataReader, TType> loadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            try
            {
                MySqlConnection lSqlConnection = new MySqlConnection(_connectionString);
                await lSqlConnection.OpenAsync();

                MySqlTransaction lTransaction = lSqlConnection.BeginTransaction(isolationLevel);

                MySqlCommand lSqlCommand = null;
                IEnumerable<MySqlParameter> lSqlParameters = sqlParameters.GetParameters(_parameterFactory);
                try
                {
                    lSqlCommand = new MySqlCommand
                    {
                        Connection = lSqlConnection,
                        Transaction = lTransaction,
                        CommandText = procedureName,
                        CommandType = CommandType.StoredProcedure
                    };
                    foreach (MySqlParameter lSqlParameter in lSqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    DbDataReader lSqlDataReader = await lSqlCommand.ExecuteReaderAsync();

                    while (await lSqlDataReader.ReadAsync())
                        simpleDataTable.AddNewRow(loadModel(lSqlDataReader));

                    await lSqlDataReader.CloseAsync();
                    await lSqlDataReader.DisposeAsync();

                    await lTransaction.CommitAsync();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog(
                        $"MySql command execution error {procedureName}", lException);
                    _logger.WriteLog(
                        $"sqlcommand:\n{string.Join("\n", lSqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())}");
                }
                finally
                {
                    await DisposeCommandAsync(lSqlCommand, lTransaction, lSqlConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MySqlProvider.StoredProc<TTYPE>:" + procedureName, lException);
            }

            return simpleDataTable;
        }


        private async Task DisposeCommandAsync(MySqlCommand sqlCommand, MySqlTransaction transaction,
            MySqlConnection sqlConnection)
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

        private void DisposeCommand(MySqlCommand sqlCommand, MySqlTransaction transaction,
            MySqlConnection sqlConnection)
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

        public List<Dictionary<string, object>> Select(string query, List<string> columns)
        {
            try
            {
                MySqlConnection lSqlConnection = new MySqlConnection(_connectionString);
                lSqlConnection.Open();

                MySqlTransaction lTransaction = lSqlConnection.BeginTransaction();

                MySqlCommand lSqlCommand = new MySqlCommand(query)
                {
                    Connection = lSqlConnection, Transaction = lTransaction
                };

                List<Dictionary<string, object>> lData = new List<Dictionary<string, object>>();

                try
                {
                    MySqlDataReader lSqlDataReader = lSqlCommand.ExecuteReader();
                    try
                    {
                        while (lSqlDataReader.Read())
                        {
                            Dictionary<string, object> lRow =
                                columns.ToDictionary(column => column, column => lSqlDataReader[column]);

                            lData.Add(lRow);
                        }

                        lSqlDataReader.Close();
                        lSqlDataReader.Dispose();

                        lTransaction.Commit();
                    }
                    catch (Exception lException)
                    {
                        _logger.WriteExceptionLog($"MySql command execution fail {query}",
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
                    _logger.WriteExceptionLog($"MySql command execution fail {query}",
                        lException);

                    lTransaction.Rollback();
                }

                DisposeCommand(lSqlCommand, lTransaction, lSqlConnection);

                return lData;
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MySqlProvider.Select", lException);
                _logger.WriteLog(query);
            }

            return null;
        }
    }
}