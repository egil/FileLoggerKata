using Moq;
using System;
using Xunit;

namespace FileLoggerKata.Tests
{
    public class FileLoggerTests
    {
        private const string TestMessage = "STANDARD_LOG_MESSAGE";
        private FileLogger Logger { get; }
        private Mock<IFileSystem> FileSystemMock { get; }
        private Mock<IDateProvider> DateProviderMock { get; }
        private DateTime DefaultToday => new DateTime(2018, 1, 1); // monday
        private string DefaultLogFileName => $"log{DefaultToday:yyyyMMdd}.txt";

        public FileLoggerTests()
        {
            FileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            FileSystemMock.Setup(fs => fs.Append(It.IsNotNull<string>(), It.IsNotNull<string>()));
            FileSystemMock.Setup(fs => fs.Create(It.IsNotNull<string>()));
            FileSystemMock.Setup(fs => fs.Exists(It.IsNotNull<string>())).Returns(true);
            FileSystemMock.Setup(fs => fs.GetLastWriteTime(It.IsNotNull<string>())).Returns(DateTime.Now);
            FileSystemMock.Setup(fs => fs.Rename(It.IsNotNull<string>(), It.IsNotNull<string>()));

            DateProviderMock = new Mock<IDateProvider>(MockBehavior.Strict);
            DateProviderMock.Setup(dp => dp.Today).Returns(DefaultToday);

            Logger = new FileLogger(FileSystemMock.Object, DateProviderMock.Object);
        }

        [Fact(DisplayName = "Log() appends message to existing log file")]
        public void LogAppendsMessageToExistingLogFile()
        {
            Logger.Log(TestMessage);

            FileSystemMock.Verify(fs => fs.Exists(DefaultLogFileName), Times.Once);
            FileSystemMock.Verify(fs => fs.Create(DefaultLogFileName), Times.Never);
            FileSystemMock.Verify(fs => fs.Append(DefaultLogFileName, TestMessage), Times.Once);
        }

        [Fact(DisplayName = "Log() creates log file if not exists and adds message")]
        public void LogCreatesLogFileIfNotExistsAndAppendsMessage()
        {
            FileSystemMock.Setup(fs => fs.Exists(DefaultLogFileName)).Returns(false);

            Logger.Log(TestMessage);

            FileSystemMock.Verify(fs => fs.Exists(DefaultLogFileName), Times.Once);
            FileSystemMock.Verify(fs => fs.Create(DefaultLogFileName), Times.Once);
            FileSystemMock.Verify(fs => fs.Append(DefaultLogFileName, TestMessage), Times.Once);
        }

        [Fact(DisplayName = "Logger users IDateProvider to determine today and logs to logYYYYMMDD.txt, " +
                            "where YYYYMMDD corresponds to the current date")]
        public void LoggerUsersIDateProviderToChooseLogFile()
        {
            Logger.Log(TestMessage);

            DateProviderMock.VerifyGet(dp => dp.Today, Times.AtLeastOnce);
            FileSystemMock.Verify(fs => fs.Append(DefaultLogFileName, TestMessage), Times.Once);
        }

        [Fact(DisplayName = "On weekends Logger logs to 'weekend.txt' instead of a day specific log file")]
        public void LoggerLogsToWeekendTxtFileOnWeekends()
        {
            var expectedLogFile = "weekend.txt";
            DateProviderMock.Setup(dp => dp.Today).Returns(new DateTime(2018, 9, 1)); // saturday

            Logger.Log(TestMessage);

            DateProviderMock.VerifyGet(dp => dp.Today, Times.AtLeastOnce);
            FileSystemMock.Verify(fs => fs.Append(expectedLogFile, TestMessage), Times.Once);
        }

        [Fact(DisplayName = "When a new weekend starts, 'weekend.txt' logs from last weekend are renamed " +
                            "to weekend-YYYYMMDD.txt, where the date points to last weekends Saturday, " +
                            "and a new weekend.txt log is created")]
        public void ArchiveWeekendTxtWhenNewWeekendStarts()
        {
            var saturday = new DateTime(2018, 9, 1);
            var lastSaturday = saturday.AddDays(-7);
            var secondOfLastSunday = lastSaturday.Add(new TimeSpan(1, 23, 59, 59));
            var expectedLogFile = "weekend.txt";
            var expectedArchivedLogFile = $"weekend-{lastSaturday:yyyyMMdd}.txt";
            DateProviderMock.Setup(dp => dp.Today).Returns(saturday);
            FileSystemMock.Setup(fs => fs.GetLastWriteTime(expectedLogFile)).Returns(secondOfLastSunday);

            Logger.Log(TestMessage);

            FileSystemMock.Verify(fs => fs.Rename(expectedLogFile, expectedArchivedLogFile), Times.Once);
            FileSystemMock.Verify(fs => fs.GetLastWriteTime(expectedLogFile), Times.Once);
        }
    }
}
