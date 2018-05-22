using System;
using System.IO;

namespace TcpAgent
{
    public class Logger
    {

        public static void Log(Exception exception, string note = "")
        {
            try
            {
                var logPath = GetLogFilePath();

                var logText = $@"{Environment.NewLine}------------------------------New Entry------------------------------
Type: ERROR
Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss.ffffff}
Note: {note}
Message: {exception?.Message}
StackTrace: {exception?.StackTrace}";

                File.AppendAllText(logPath, logText);
            }
            catch
            {
                // Ignored
            }
        }

        public static void Log(string logText, string type = "INFO")
        {
            try
            {
                var logPath = GetLogFilePath();

                var logTextInfo = $@"{Environment.NewLine}------------------------------New Entry------------------------------
Type: {type}
Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss.ffffff}
Message: {logText}";

                File.AppendAllText(logPath, logTextInfo);
            }
            catch
            {
                // Ignored
            }
        }

        public static void LogInfo(string logText)
        {
            try
            {
                var logPath = GetLogFilePath("Info");

                var logTextInfo = $@"{Environment.NewLine}------------------------------New Entry------------------------------
Type: INFO
Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss.ffffff}
Message: {logText}";

                File.AppendAllText(logPath, logTextInfo);
            }
            catch
            {
                // Ignored
            }
        }

        private static string GetLogFilePath(string prefix = "")
        {
            try
            {
                var folder = Directory.GetCurrentDirectory() + "\\logs";

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                return Path.Combine(folder, $"{prefix}Log_{DateTime.Now:yyyyMMdd}.txt");
            }
            catch
            {
                // ignored
            }
            return string.Empty;
        }
    }
}
