#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using DbProviderWrapper.Interfaces;
using DbProviderWrapper.Persistance;

#endregion

namespace DbProviderWrapper.MsSql
{
    public class MsSqlProvider : IMsSqlProvider
    {
        #region Fields

        private readonly string _connectionString;
        private IDbLogger _logger;

        #endregion

        #region Constructors

        public MsSqlProvider(IDbLogger logger, IDbConnectionStringProvider connectionStringProvider)
        {
            _logger = logger;
            _connectionString = connectionStringProvider.GetMsSqlConnectionString();
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

                SqlCommand lSqlCommand = new SqlCommand(query);
                lSqlCommand.Connection = lSqlConnection;
                lSqlCommand.Transaction = lTransaction;

                List<IDictionary<string, object>> lData = new List<IDictionary<string, object>>();

                try
                {
                    SqlDataReader lSqlDataReader = lSqlCommand.ExecuteReader();
                    try
                    {
                        while (lSqlDataReader.Read())
                        {
                            Dictionary<string, object> lRow = new Dictionary<string, object>();

                            foreach (string lColumn in columns)
                                lRow.Add(lColumn, lSqlDataReader[lColumn]);

                            lData.Add(lRow);
                        }

                        lSqlDataReader.Close();
                        lSqlDataReader.Dispose();

                        lTransaction.Commit();
                    }
                    catch (Exception lException)
                    {
                        _logger.WriteExceptionLog(string.Format("MsSql command execution fail {0}", query),
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
                    _logger.WriteExceptionLog(string.Format("MsSql command execution error {0}", query), lException);

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

                SqlCommand lSqlCommand = new SqlCommand(query);
                lSqlCommand.Connection = lSqlConnection;
                lSqlCommand.Transaction = lTransaction;

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
                        _logger.WriteExceptionLog(string.Format("MsSql command execution fail {0}", query),
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
                    _logger.WriteExceptionLog(string.Format("MsSql command execution error {0}", query), lException);

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

        public SqlDataReader StoredProc(string procedureName, IEnumerable<SqlParameter> sqlParameters)
        {
            try
            {
                SqlConnection lSqlConnection = new SqlConnection(_connectionString);
                lSqlConnection.Open();

                SqlTransaction lTransaction = lSqlConnection.BeginTransaction();

                SqlCommand lSqlCommand = null;

                try
                {
                    lSqlCommand = new SqlCommand();

                    lSqlCommand.Connection = lSqlConnection;
                    lSqlCommand.Transaction = lTransaction;

                    lSqlCommand.CommandText = procedureName;
                    lSqlCommand.CommandType = CommandType.StoredProcedure;

                    foreach (SqlParameter lSqlParameter in sqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    lSqlCommand.ExecuteNonQuery();

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog(string.Format("MsSql command execution error {0}", procedureName),
                        lException);
                    _logger.WriteLog(string.Format("Parameters:\n{0}",
                        string.Join("\n", sqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())));
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

        public async Task<SqlDataReader> StoredProcAsync(string procedureName, IEnumerable<SqlParameter> sqlParameters)
        {
            try
            {
                SqlConnection lSqlConnection = new SqlConnection(_connectionString);
                await lSqlConnection.OpenAsync();

                SqlTransaction lTransaction = lSqlConnection.BeginTransaction();

                SqlCommand lSqlCommand = null;

                try
                {
                    lSqlCommand = new SqlCommand
                    {
                        Connection = lSqlConnection,
                        Transaction = lTransaction,
                        CommandText = procedureName,
                        CommandType = CommandType.StoredProcedure
                    };


                    foreach (SqlParameter lSqlParameter in sqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    await lSqlCommand.ExecuteNonQueryAsync();

                    await lTransaction.CommitAsync();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog(string.Format("MsSql command execution error {0}", procedureName),
                        lException);
                    _logger.WriteLog(string.Format("Parameters:\n{0}",
                        string.Join("\n", sqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())));
                    await lTransaction.RollbackAsync();
                }
                finally
                {
                    await DisposeCommandAsync(lSqlCommand, lTransaction, lSqlConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MsSqlProvider.StoredProc:" + procedureName, lException);
            }

            return null;
        }

        public void StoredProc<TTYPE>(
            string procedureName,
            IEnumerable<SqlParameter> sqlParameters,
            ref SimpleDataTable<TTYPE> simpleDataTable,
            Func<SqlDataReader, TTYPE> LoadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            try
            {
                SqlConnection lSqlConnection = new SqlConnection(_connectionString);
                lSqlConnection.Open();

                SqlTransaction lTransaction = lSqlConnection.BeginTransaction(isolationLevel);

                SqlCommand lSqlCommand = null;

                try
                {
                    lSqlCommand = new SqlCommand();

                    lSqlCommand.Connection = lSqlConnection;
                    lSqlCommand.Transaction = lTransaction;

                    lSqlCommand.CommandText = procedureName;
                    lSqlCommand.CommandType = CommandType.StoredProcedure;

                    foreach (SqlParameter lSqlParameter in sqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    SqlDataReader lSqlDataReader = lSqlCommand.ExecuteReader();

                    while (lSqlDataReader.Read())
                        simpleDataTable.AddNewRow(LoadModel(lSqlDataReader));

                    lSqlDataReader.Close();
                    lSqlDataReader.Dispose();

                    lTransaction.Commit();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog(string.Format("MsSql command execution error {0}", procedureName),
                        lException);
                    _logger.WriteLog(string.Format("Parameters:\n{0}",
                        string.Join("\n", sqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())));
                }
                finally
                {
                    DisposeCommand(lSqlCommand, lTransaction, lSqlConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MsSqlProvider.StoredProc<TTYPE>:" + procedureName, lException);
            }
        }

        public async Task<SimpleDataTable<TTYPE>> StoredProcAsync<TTYPE>(
            string procedureName,
            IEnumerable<SqlParameter> sqlParameters,
            SimpleDataTable<TTYPE> simpleDataTable,
            Func<SqlDataReader, TTYPE> LoadModel, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            try
            {
                SqlConnection lSqlConnection = new SqlConnection(_connectionString);
                await lSqlConnection.OpenAsync();

                SqlTransaction lTransaction = lSqlConnection.BeginTransaction(isolationLevel);

                SqlCommand lSqlCommand = null;

                try
                {
                    lSqlCommand = new SqlCommand
                    {
                        Connection = lSqlConnection,
                        Transaction = lTransaction,
                        CommandText = procedureName,
                        CommandType = CommandType.StoredProcedure
                    };
                    foreach (SqlParameter lSqlParameter in sqlParameters)
                        lSqlCommand.Parameters.Add(lSqlParameter);

                    SqlDataReader lSqlDataReader = await lSqlCommand.ExecuteReaderAsync();

                    while (await lSqlDataReader.ReadAsync())
                        simpleDataTable.AddNewRow(LoadModel(lSqlDataReader));

                    await lSqlDataReader.CloseAsync();
                    await lSqlDataReader.DisposeAsync();

                    await lTransaction.CommitAsync();
                }
                catch (Exception lException)
                {
                    _logger.WriteExceptionLog(string.Format("MsSql command execution error {0}", procedureName),
                        lException);
                    _logger.WriteLog(string.Format("Parameters:\n{0}",
                        string.Join("\n", sqlParameters.Select(x => x.ParameterName + "=" + x.Value).ToArray())));
                }
                finally
                {
                    await DisposeCommandAsync(lSqlCommand, lTransaction, lSqlConnection);
                }
            }
            catch (Exception lException)
            {
                _logger.WriteExceptionLog("MsSqlProvider.StoredProc<TTYPE>:" + procedureName, lException);
            }

            return simpleDataTable;
        }

        private async Task DisposeCommandAsync(SqlCommand lSqlCommand, SqlTransaction lTransaction,
            SqlConnection lSqlConnection)
        {
            if (lSqlCommand != null)
            {
                lSqlCommand.Parameters.Clear();
                await lSqlCommand.DisposeAsync();
            }

            try
            {
                await lTransaction.DisposeAsync();
            }
            catch
            {
            }

            try
            {
                await lSqlConnection.CloseAsync();
                await lSqlConnection.DisposeAsync();
            }
            catch
            {
            }
        }

        private void DisposeCommand(SqlCommand lSqlCommand, SqlTransaction lTransaction, SqlConnection lSqlConnection)
        {
            if (lSqlCommand != null)
            {
                lSqlCommand.Parameters.Clear();
                lSqlCommand.Dispose();
            }

            try
            {
                lTransaction.Dispose();
            }
            catch
            {
            }

            try
            {
                lSqlConnection.Close();
                lSqlConnection.Dispose();
            }
            catch
            {
            }
        }
    }
}