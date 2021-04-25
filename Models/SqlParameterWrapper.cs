using DbProviderWrapper.Builders;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.Models
{
    public class SqlParameterWrapper : ISqlParameter
    {
        #region Fields

        private readonly ISqlParameter _sqlParameter;

        #endregion

        #region Constructors

        public SqlParameterWrapper(ISqlParameter sqlParameter)
        {
            _sqlParameter = sqlParameter;
        }

        public SqlParameterWrapper(string name, object value)
        {
            _sqlParameter = new SqlParameter<object>(name, value);
        }

        #endregion

        public string Name
        {
            get { return _sqlParameter.Name; }
        }

        public object Value
        {
            get { return _sqlParameter.Value; }
        }

        TParameter ISqlParameter.GetParameter<TParameter>(IParameterFactory<TParameter> factory)
        {
            return _sqlParameter.GetParameter(factory);
        }
    }
}