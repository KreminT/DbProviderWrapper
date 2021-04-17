using DbProviderWrapper.Builders;
using MySql.Data.MySqlClient;

namespace DbProviderWrapper.MySql
{
    public class MySqlParameterFactory : IParameterFactory<MySqlParameter>
    {
        public MySqlParameter CreateParameter<TValue>(string name, TValue value)
        {
            return new MySqlParameter(name, value);
        }
    }
}