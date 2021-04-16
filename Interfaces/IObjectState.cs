using DbProviderWrapper.Persistence;

namespace DbProviderWrapper.Interfaces
{
    public interface IObjectState
    {
        public ObjectState ObjectStatus { get; }

        public void SetStatus(ObjectState status);
    }
}