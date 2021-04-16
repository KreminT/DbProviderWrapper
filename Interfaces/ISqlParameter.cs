namespace DbProviderWrapper.Interfaces
{
    public interface ISqlParameter
    {
        internal TParameter GetParameter<TParameter>(IParameterFactory<TParameter> factory);
    }
}