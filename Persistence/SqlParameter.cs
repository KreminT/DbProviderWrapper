using DbProviderWrapper.Interfaces;

namespace DbProviderWrapper.Persistence
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

        protected string Name
        {
            get { return _name; }
        }

        protected TValue Value
        {
            get { return _value; }
        }

        #endregion


        TParameter ISqlParameter.GetParameter<TParameter>(IParameterFactory<TParameter> factory)
        {
            return factory.CreateParameter(_name,_value);
        }
    }
}