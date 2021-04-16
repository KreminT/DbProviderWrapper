namespace DbProviderWrapper.Interfaces
{
    public interface IParameterFactory<out TParameter>
    {
        TParameter CreateParameter<TValue>(string name,TValue value);
    }
}