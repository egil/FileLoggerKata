using System;

namespace FileLoggerKata
{
    public interface IDateProvider
    {
        DateTime Today { get; }
    }
}