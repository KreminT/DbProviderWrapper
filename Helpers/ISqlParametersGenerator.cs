using System.Collections.Generic;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.Helpers
{
    public interface ISqlParametersGenerator
    {
        List<ISqlParameter> BuildSqlParameters<TTypeArgs>(TTypeArgs args);
    }
}