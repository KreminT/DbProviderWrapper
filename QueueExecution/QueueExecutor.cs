using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using DbProviderWrapper.Helpers;
using DbProviderWrapper.Models;
using DbProviderWrapper.Models.Interfaces;
using Microsoft.Extensions.Logging;

namespace DbProviderWrapper.QueueExecution
{
    public class QueueExecutor
    {
        #region Fields
        
        private readonly ILogger _logger;
        private readonly IDbQueueProvider _provider;

        private Queue<ISqlQueueItem> _items = new Queue<ISqlQueueItem>();

        #endregion

        #region Constructors

        public QueueExecutor(ILogger logger, IDbQueueProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        #endregion

        public void AddToQueue<T>(ISqlQueued<T> persistence, T model) where T : IObjectState
        {
            switch (model.ObjectStatus)
            {
                case ObjectState.Created:
                    _items.Enqueue(persistence.GetSaveQueueItem(model));
                    break;
                case ObjectState.Modified:
                    _items.Enqueue(persistence.GetUpdateQueueItem(model));
                    break;
                case ObjectState.Deleted:
                    _items.Enqueue(persistence.GetDeleteQueueItem(model));
                    break;
            }
        }

        public void AddToQueue(string procedure, IEnumerable<ISqlParameter> parameters)
        {
            _items.Enqueue(new SqlQueueItem(parameters, procedure));
        }

        public async Task<bool> Execute()
        {
            ISqlQueueItem item;
            bool lResult = true;

            DbConnection lConnection = _provider.CreateConnection();
            DbTransaction lTransaction = null;
            try
            {
                await lConnection.OpenAsync();
                lTransaction = await lConnection.BeginTransactionAsync();
                while (_items.Any() && (item = _items.Dequeue()) != null)
                {
                    lResult &= await _provider
                        .StoredProcAsync(item.CommandName, item.Parameters, lConnection, lTransaction);
                    if (!lResult)
                        break;
                }

                if (lResult)
                    await lTransaction.CommitAsync();
                await lConnection.CloseAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("Queue Executor", e);
                lResult = false;
            }
            finally
            {
                await _provider.DisposeConnectionAsync(lTransaction, lConnection);
            }

            return lResult;
        }
    }
}