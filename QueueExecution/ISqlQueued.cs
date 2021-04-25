namespace DbProviderWrapper.QueueExecution
{
    public interface ISqlQueued<in TType>
    {
        ISqlQueueItem GetDeleteQueueItem(TType model);
        ISqlQueueItem GetSaveQueueItem(TType model);
        ISqlQueueItem GetUpdateQueueItem(TType model);
    }
}