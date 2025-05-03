using System;
using System.IO;
using System.Text;

namespace DungeonFlux.Model
{
    public static class Logger
    {
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        private static readonly string LogDirectory = "Logs";
        private static readonly string LogFileName = "debug.log";
        private static readonly string LogFilePath = Path.Combine(LogDirectory, LogFileName);
        private static readonly object LockObject = new object();
        private static readonly int MaxLogFileSize = 5 * 1024 * 1024; // 5 MB
        private static readonly int MaxLogFiles = 5;

        static Logger()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                // Очищаем файл при запуске
                File.WriteAllText(LogFilePath, $"{DateTime.Now} Log started\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize logger: {ex.Message}");
            }
        }


        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            lock (LockObject)
            {
                try
                {
                    CheckLogFileSize();
                    var formattedMessage = FormatMessage(message, level);
                    File.AppendAllText(LogFilePath, formattedMessage);
                }
                catch (Exception ex)
                {
                    // Если не удалось записать в лог, хотя бы выведем в консоль
                    Console.WriteLine($"Failed to write to log: {ex.Message}");
                    Console.WriteLine($"Original message: {message}");
                }
            }
        }


        public static void LogWarning(string message)
        {
            Log(message, LogLevel.Warning);
        }

        public static void LogError(string message, Exception ex = null)
        {
            var errorMessage = $"ERROR: {message}";
            if (ex != null)
            {
                errorMessage += $"\nException: {ex.Message}\nStack trace: {ex.StackTrace}";
            }
            Log(errorMessage, LogLevel.Error);
        }


        private static string FormatMessage(string message, LogLevel level)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var levelStr = level.ToString().ToUpper();
            return $"[{timestamp}] [{levelStr}] {message}\n";
        }


        private static void CheckLogFileSize()
        {
            if (!File.Exists(LogFilePath))
                return;

            var fileInfo = new FileInfo(LogFilePath);
            if (fileInfo.Length < MaxLogFileSize)
                return;

            // Ротация логов
            for (int i = MaxLogFiles - 1; i > 0; i--)
            {
                var currentFile = Path.Combine(LogDirectory, $"debug.{i}.log");
                var nextFile = Path.Combine(LogDirectory, $"debug.{i + 1}.log");

                if (File.Exists(currentFile))
                {
                    if (i == MaxLogFiles - 1)
                    {
                        File.Delete(currentFile);
                    }
                    else
                    {
                        File.Move(currentFile, nextFile);
                    }
                }
            }

            File.Move(LogFilePath, Path.Combine(LogDirectory, "debug.1.log"));
            File.WriteAllText(LogFilePath, $"{DateTime.Now} Log rotated\n");
        }


        public static void ClearLogs()
        {
            lock (LockObject)
            {
                try
                {
                    if (Directory.Exists(LogDirectory))
                    {
                        foreach (var file in Directory.GetFiles(LogDirectory, "*.log"))
                        {
                            File.Delete(file);
                        }
                    }
                    File.WriteAllText(LogFilePath, $"{DateTime.Now} Logs cleared\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to clear logs: {ex.Message}");
                }
            }
        }
    }
} 