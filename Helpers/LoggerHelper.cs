// /Helpers/LoggerHelper.cs
namespace MyProject.Helpers
{
    public enum LogLevelType
    {
        Information,
        Warning,
        Error,
        Critical,
        Debug
    }

    public class LoggerHelper
    {
        private readonly ILogger<LoggerHelper> _logger;

        public LoggerHelper(ILogger<LoggerHelper> logger)
        {
            _logger = logger;
        }

        public void LogMessage(LogLevelType logLevelType, string message)
        {
            // Map the LogLevelType to actual ILogger log level
            switch (logLevelType)
            {
                case LogLevelType.Information:
                    _logger.LogInformation($"[INFO] {message}");
                    break;

                case LogLevelType.Warning:
                    _logger.LogWarning($"[WARN] {message}");
                    break;

                case LogLevelType.Error:
                    _logger.LogError($"[ERROR] {message}");
                    break;

                case LogLevelType.Critical:
                    _logger.LogCritical($"[CRITICAL] {message}");
                    break;

                case LogLevelType.Debug:
                    _logger.LogDebug($"[DEBUG] {message}");
                    break;
            }
        }
    }
}
