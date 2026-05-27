using System;
using System.IO;
using static PPPredictor.Core.DataType.Enums;

namespace PPPredictor.Core
{
    internal class Logging
    {
        private const string LogFilePath = "/var/home/nub/.local/share/Steam/steamapps/common/BeatSaberDev/1_40_7/Beat Saber/UserData/PPPLog.txt";
        private static readonly object LogFileLock = new object();

        public static event EventHandler<LoggingMessage> OnMessage;

        
        //TODO Remove
        internal static void LogToFile(string message)
        {
            var logDirectory = Path.GetDirectoryName(LogFilePath);
            var timestampedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}";

            lock (LogFileLock)
            {
                if (!string.IsNullOrEmpty(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                File.AppendAllText(LogFilePath, timestampedMessage);
            }
        }

        internal static void ErrorPrint(string message)
        {
            OnMessage?.Invoke(null, new LoggingMessage(LoggingMessage.LoggingType.Error, message));
        }

        internal static void LoadingStatusPrint(Leaderboard leaderboard, string message)
        {
            OnMessage?.Invoke(null, new LoggingMessage(LoggingMessage.LoggingType.LoadingStatus, leaderboard, message));
        }

        internal static void DebugNetworkPrint(string message, Leaderboard leaderboard)
        {
            OnMessage?.Invoke(null, new LoggingMessage(LoggingMessage.LoggingType.DebugNetworkPrint, leaderboard, message));
        }
    }

    public class LoggingMessage
    {
        public LoggingType loggingType;
        public string message;
        public Leaderboard leaderboard;

        public LoggingMessage(LoggingType loggingType, string message)
        {
            this.loggingType = loggingType;
            this.message = message;
            this.leaderboard = Leaderboard.NoLeaderboard;
        }

        public LoggingMessage(LoggingType loggingType, Leaderboard leaderboard, string message)
        {
            this.loggingType = loggingType;
            this.message = message;
            this.leaderboard = leaderboard;
        }

        public enum LoggingType
        {
            Error,
            DebugNetworkPrint,
            LoadingStatus
        }
    }
}
