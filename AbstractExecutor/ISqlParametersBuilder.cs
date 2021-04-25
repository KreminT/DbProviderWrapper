using System.Collections.Generic;
using DbProviderWrapper.Models.Interfaces;

namespace DbProviderWrapper.AbstractExecutor
{
    public interface ISqlParametersBuilder
    {
        List<ISqlParameter> BuildSqlParameters<TTypeArgs>(TTypeArgs args);
    }
}