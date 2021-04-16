#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Interfaces;
using DbProviderWrapper.Persistence;

#endregion

namespace DbProviderWrapper.MsSql
{
    public class MsSqlProvider : IDbProvider
    {
        #region Fields

        private readonly string _connectionString;
        private readonly IDbLogger _logger;
        private readonly IParameterFactory<SqlParameter> _parameterFactory;

        #endregion

        #region Constructors

        public MsSqlProvider(IDbLogger logger, IMsSqlConnectionStringProvider connectionStringProvider)
        {
            _logger = logger;
            _connectionString = connectionStringProvider.GetMsSqlConnectionString();
            _parameterFactory = new MsSqlParameterFactory();
        }

        #endregion

        public IDbLogger Logger
        {
            get { return _logger; }
        }

        public List<IDictionary<string, object>> Select(string query, IEnumerable<string> columns)
        {
            try
            {
                SqlConnection lSqlConnection = new SqlConnection(_connectionString);
                lSqlConnection.Open();

                SqlTransaction lTransaction = lSqlConnection.BeginTransaction();

                SqlCommand lSqlCommand = new SqlCommand(query)
                {
                    Connection = lSqlConnection, Transaction = lTransaction
                };

                List<IDictionary<string, object>> lData = new List<IDictionary<string, object>>();

                try
                {
                    SqlDataReader lSqlDataReader = lSqlCommand.ExecuteReader();
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
                        _logger.WriteExceptionLog($"MsSql command execution fail {query}",
                            lException);

                        lTransaction.Rollback();
                    }
                    finally
                    {
                        lSqlDataReader.Close();
                        lSqlDataReader.Dispose();
                        lSqlDataReader = null;
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
                SqlConnection lSqlConnection = new SqlConnection(_connectionString);
                lSqlConnection.Open();

                SqlTransaction lTransaction = lSqlConnection.BeginTransaction();

                SqlCommand lSqlCommand = new SqlCommand(query)
                {
                    Connection = lSqlConnection, Transaction = lTransaction
                };

                try
                {
                    SqlDataReader lSqlDataReader = lSqlCommand.ExecuteReader();

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
                        _logger.WriteExceptionLog($"MsSql command execution fail {query}",
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
                    _logger.WriteExceptionLog($"MsSql command execution error {query}", lException);

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
                _logger.WriteExceptionLog("MsSqlProvider.SimpleSelect", lException);
            }

            return null;
        }

        public IDataReader StoredProc(string procedureName, IEnumerable<ISqlParameter> sqlParameters)
        {
            try
            {
                SqlConnection lSqlConnection = new SqlConnection(_connectionString);
                lSqlConnection.Open();

                SqlTransaction lTransaction = lSqlConnection.BeginTransaction();

                SqlCommand lSqlCommand = null;

                IEnumerable<SqlParameter> lSqlParameters = sqlParameters.GetParameters(_parameterFactory);
                try
                {
                    lSqlCommand = new SqlCommand
                    {
                        Connection = lSqlConnection,
                        Transaction = lTransaction,
                        CommandText = procedureName,
                        CommandType = CommandType.StoredProcedure
                    };


                    foreach (SqlParameter lSqlParameter in lSqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    lSqlCommand.ExecuteNonQuery();

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog($"MsSql command execution error {procedureName}",
                        lException);
                    _logger.WriteLog(
                        $"Parameters:\n{string.Join("\n", lSqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())}");
                    lTransaction.Rollback();
                }
                finally
                {
                    DisposeCommand(lSqlCommand, lTransaction, lSqlConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MsSqlProvider.StoredProc:" + procedureName, lException);
            }

            return null;
        }

        public async Task<IDataReader> StoredProcAsync(string procedureName,
            IEnumerable<ISqlParameter> sqlParameters)
        {
            try
            {
                SqlConnection lSqlConnection = new SqlConnection(_connectionString);
                await lSqlConnection.OpenAsync();

                SqlTransaction lTransaction = lSqlConnection.BeginTransaction();

                SqlCommand lSqlCommand = null;

                IEnumerable<SqlParameter> lSqlParameters = sqlParameters.GetParameters(_parameterFactory);
                try
                {
                    lSqlCommand = new SqlCommand
                    {
                        Connection = lSqlConnection,
                        Transaction = lTransaction,
                        CommandText = procedureName,
                        CommandType = CommandType.StoredProcedure
                    };


                    foreach (SqlParameter lSqlParameter in lSqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    await lSqlCommand.ExecuteNonQueryAsync();

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog($"MsSql command execution error {procedureName}",
                        lException);
                    _logger.WriteLog(
                        $"Parameters:\n{string.Join("\n", lSqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())}");
                    lTransaction.Rollback();
                }
                finally
                {
                    DisposeCommandAsync(lSqlCommand, lTransaction, lSqlConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MsSqlProvider.StoredProc:" + procedureName, lException);
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
                SqlConnection lSqlConnection = new SqlConnection(_connectionString);
                lSqlConnection.Open();

                SqlTransaction lTransaction = lSqlConnection.BeginTransaction(isolationLevel);

                SqlCommand lSqlCommand = null;

                IEnumerable<SqlParameter> lSqlParameters = sqlParameters.GetParameters(_parameterFactory);
                try
                {
                    lSqlCommand = new SqlCommand
                    {
                        Connection = lSqlConnection,
                        Transaction = lTransaction,
                        CommandText = procedureName,
                        CommandType = CommandType.StoredProcedure
                    };


                    foreach (SqlParameter lSqlParameter in lSqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    SqlDataReader lSqlDataReader = lSqlCommand.ExecuteReader();

                    while (lSqlDataReader.Read())
                        simpleDataTable.AddNewRow(loadModel(lSqlDataReader));

                    lSqlDataReader.Close();
                    lSqlDataReader.Dispose();

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog($"MsSql command execution error {procedureName}",
                        lException);
                    _logger.WriteLog(
                        $"Parameters:\n{string.Join("\n", lSqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())}");
                }
                finally
                {
                    DisposeCommand(lSqlCommand, lTransaction, lSqlConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MsSqlProvider.StoredProc<TType>:" + procedureName, lException);
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
                SqlConnection lSqlConnection = new SqlConnection(_connectionString);
                await lSqlConnection.OpenAsync();

                SqlTransaction lTransaction = lSqlConnection.BeginTransaction(isolationLevel);

                SqlCommand lSqlCommand = null;

                IEnumerable<SqlParameter> lSqlParameters = sqlParameters.GetParameters(_parameterFactory);
                try
                {
                    lSqlCommand = new SqlCommand
                    {
                        Connection = lSqlConnection,
                        Transaction = lTransaction,
                        CommandText = procedureName,
                        CommandType = CommandType.StoredProcedure
                    };
                    foreach (SqlParameter lSqlParameter in lSqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    SqlDataReader lSqlDataReader = await lSqlCommand.ExecuteReaderAsync();

                    while (await lSqlDataReader.ReadAsync())
                        simpleDataTable.AddNewRow(loadModel(lSqlDataReader));

                    lSqlDataReader.Close();
                    lSqlDataReader.Dispose();

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog($"MsSql command execution error {procedureName}",
                        lException);
                    _logger.WriteLog(
                        $"Parameters:\n{string.Join("\n", lSqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())}");
                }
                finally
                {
                    DisposeCommandAsync(lSqlCommand, lTransaction, lSqlConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MsSqlProvider.StoredProc<TTYPE>:" + procedureName, lException);
            }

            return simpleDataTable;
        }

        private void DisposeCommandAsync(SqlCommand sqlCommand, SqlTransaction transaction,
            SqlConnection sqlConnection)
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
            }

            try
            {
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
            catch
            {
            }
        }

        private void DisposeCommand(SqlCommand sqlCommand, SqlTransaction transaction, SqlConnection sqlConnection)
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
            }

            try
            {
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
            catch
            {
            }
        }
    }
}