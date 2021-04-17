using DbProviderWrapper.Builders;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.Models
{
    public class SqlParameter<TValue> : ISqlParameter
    {
        #region Fields

        private readonly string _name;
        private readonly TValue _value;

        #endregion

        #region Constructors

        protected SqlParameter(string name, TValue value)
        {
            _name = name;
            _value = value;
        }

        #endregion

        #region Properties

        public TValue Value
        {
            get { return _value; }
        }

        #endregion

        public string Name
        {
            get { return _name; }
        }

        object ISqlParameter.Value
        {
            get { return Value; }
        }


        TParameter ISqlParameter.GetParameter<TParameter>(IParameterFactory<TParameter> factory)
        {
            return factory.CreateParameter(_name, _value);
        }
    }
}