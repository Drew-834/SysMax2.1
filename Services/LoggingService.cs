using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SysMax2._1.Services
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }

    /// <summary>
    /// Service that handles logging of system events and actions in a user-friendly format
    /// </summary>
    public class LoggingService
    {
        private static LoggingService? _instance;
        private readonly string _logFilePath;
        private readonly object _lockObj = new object();
        private StringBuilder _recentLogs = new StringBuilder();

        // Maximum number of recent logs to keep in memory
        private const int MaxRecentLogs = 100;
        private int _recentLogCount = 0;

        private LoggingService()
        {
            // Create logs directory if it doesn't exist
            string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Directory.CreateDirectory(logsDirectory);

            // Create a log file with current date in name
            string fileName = $"SysMax_Log_{DateTime.Now:yyyy-MM-dd}.txt";
            _logFilePath = Path.Combine(logsDirectory, fileName);

            // Log application start
            Log(LogLevel.Info, "Application started");
        }

        /// <summary>
        /// Gets the singleton instance of the LoggingService
        /// </summary>
        public static LoggingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoggingService();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Logs a message with the specified log level
        /// </summary>
        /// <param name="level">The severity level of the log</param>
        /// <param name="message">The message to log</param>
        public void Log(LogLevel level, string message)
        {
            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

            // Add to recent logs in memory
            lock (_lockObj)
            {
                _recentLogs.AppendLine(formattedMessage);
                _recentLogCount++;

                // If we've exceeded the maximum number of logs, trim the oldest ones
                if (_recentLogCount > MaxRecentLogs)
                {
                    // Find the position after the first newline
                    int pos = _recentLogs.ToString().IndexOf(Environment.NewLine);
                    if (pos >= 0)
                    {
                        _recentLogs.Remove(0, pos + Environment.NewLine.Length);
                        _recentLogCount--;
                    }
                }
            }

            // Write to file
            try
            {
                lock (_lockObj)
                {
                    File.AppendAllText(_logFilePath, formattedMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs a message asynchronously
        /// </summary>
        /// <param name="level">The severity level of the log</param>
        /// <param name="message">The message to log</param>
        public async Task LogAsync(LogLevel level, string message)
        {
            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

            // Add to recent logs in memory
            lock (_lockObj)
            {
                _recentLogs.AppendLine(formattedMessage);
                _recentLogCount++;

                // If we've exceeded the maximum number of logs, trim the oldest ones
                if (_recentLogCount > MaxRecentLogs)
                {
                    // Find the position after the first newline
                    int pos = _recentLogs.ToString().IndexOf(Environment.NewLine);
                    if (pos >= 0)
                    {
                        _recentLogs.Remove(0, pos + Environment.NewLine.Length);
                        _recentLogCount--;
                    }
                }
            }

            // Write to file asynchronously
            try
            {
                await File.AppendAllTextAsync(_logFilePath, formattedMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the most recent logs that are stored in memory
        /// </summary>
        /// <returns>A string containing the recent logs</returns>
        public string GetRecentLogs()
        {
            lock (_lockObj)
            {
                return _recentLogs.ToString();
            }
        }

        /// <summary>
        /// Gets all logs from the current log file
        /// </summary>
        /// <returns>A string containing all logs</returns>
        public async Task<string> GetAllLogs()
        {
            try
            {
                return await File.ReadAllTextAsync(_logFilePath);
            }
            catch (Exception ex)
            {
                return $"Error reading log file: {ex.Message}";
            }
        }

        /// <summary>
        /// Clears the in-memory recent logs
        /// </summary>
        public void ClearRecentLogs()
        {
            lock (_lockObj)
            {
                _recentLogs.Clear();
                _recentLogCount = 0;
            }
        }

        /// <summary>
        /// Gets all available log files
        /// </summary>
        /// <returns>An array of log file names</returns>
        public string[] GetLogFiles()
        {
            try
            {
                string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                return Directory.GetFiles(logsDirectory, "SysMax_Log_*.txt");
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Gets the logs from a specific file
        /// </summary>
        /// <param name="filePath">The full path to the log file</param>
        /// <returns>A string containing the logs from the file</returns>
        public async Task<string> GetLogsFromFile(string filePath)
        {
            try
            {
                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                return $"Error reading log file: {ex.Message}";
            }
        }
    }
}