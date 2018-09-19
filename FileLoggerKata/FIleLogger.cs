using System;

namespace FileLoggerKata
{
    public class FileLogger
    {
        private const string LogExtension = "txt";
        private const string LogFileName = "log";
        private const string WeekendLogFileName = "weekend";

        private IDateProvider DateProvider { get; }
        private IFileSystem FileSystem { get; }

        public FileLogger(IFileSystem fileSystem, IDateProvider dateProvider = null)
        {
            DateProvider = dateProvider ?? DefaultDataProvider.Instance;
            FileSystem = fileSystem;
        }

        public void Log(string message)
        {
            var logFileName = GetLogFileName();

            if (ShouldRotateWeekendLogs())
            {
                ArchiveWeekendLog();
            }

            if (!FileSystem.Exists(logFileName))
            {
                FileSystem.Create(logFileName);
            }

            FileSystem.Append(logFileName, message);

            bool ShouldRotateWeekendLogs()
            {
                return DateProvider.Today.DayOfWeek == DayOfWeek.Saturday && 
                       FileSystem.Exists(logFileName) &&
                       FileSystem.GetLastWriteTime(logFileName).DayOfWeek == DayOfWeek.Sunday;
            }
        }
        
        private string GetLogFileName()
        {
            var today = DateProvider.Today;

            return IsWeekend()
                ? $"{WeekendLogFileName}.{LogExtension}"
                : $"{LogFileName}{ToFileDateFormat(today)}.{LogExtension}";

            bool IsWeekend() => today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday;
        }

        private void ArchiveWeekendLog()
        {            
            var lastSaturday = DateProvider.Today.AddDays(-7);
            var archivedFileName = $"{WeekendLogFileName}-{ToFileDateFormat(lastSaturday)}.{LogExtension}";
            FileSystem.Rename($"{WeekendLogFileName}.{LogExtension}", archivedFileName);
        }

        private static string ToFileDateFormat(DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }
    }
}
