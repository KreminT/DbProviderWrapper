using System;

namespace DbProviderWrapper.Interfaces
{
    public interface IDbLogger
    {
        void WriteExceptionLog(string message, Exception exception);
        void WriteLog(string message);
    }
}