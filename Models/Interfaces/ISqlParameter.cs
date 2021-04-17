using DbProviderWrapper.Builders;

namespace DbProviderWrapper.Models.Interfaces
{
    public interface ISqlParameter
    {
        #region Properties

        string Name { get; }
        object Value { get; }

        #endregion

        internal TParameter GetParameter<TParameter>(IParameterFactory<TParameter> factory);
    }
}