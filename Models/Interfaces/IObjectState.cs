namespace DbProviderWrapper.Models.Interfaces
{
    public interface IObjectState
    {
        public ObjectState ObjectStatus { get; }

        public void SetStatus(ObjectState status);
    }
}