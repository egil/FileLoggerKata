using System;

namespace FileLoggerKata
{
    public class DefaultDataProvider : IDateProvider {
        public static IDateProvider Instance => new DefaultDataProvider();

        public DateTime Today => DateTime.Today;
    }
}