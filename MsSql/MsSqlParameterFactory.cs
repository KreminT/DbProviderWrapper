using System.Data.SqlClient;
using DbProviderWrapper.Interfaces;

namespace DbProviderWrapper.MsSql
{
    public class MsSqlParameterFactory:IParameterFactory<SqlParameter>
    {
        public SqlParameter CreateParameter<TValue>(string name, TValue value)
        {
            return new SqlParameter(name,value);
        }
    }
}