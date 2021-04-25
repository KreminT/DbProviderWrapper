using System;

namespace DbProviderWrapper.AbstractExecutor.Attributes
{
    public class DbColumn
        : Attribute
    {
        #region Fields

        private string _name;

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        #endregion
    }
}