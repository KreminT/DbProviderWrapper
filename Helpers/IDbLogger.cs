using System;

namespace DbProviderWrapper.Helpers
{
    public interface IDbLogger
    {
        void WriteExceptionLog(string message, Exception exception);
        void WriteLog(string message);
    }
}