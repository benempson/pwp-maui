using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace PWP.Maui.Utils.Extensions;

public static class LoggerExtensions
{
    public static void LogAtDefinedLevel(this ILogger logger, string message, LogLevel logLevel, params object[] args)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                logger.LogTrace(message, args);
                break;

            case LogLevel.Debug:
                logger.LogDebug(message, args);
                break;

            case LogLevel.Information:
                logger.LogInformation(message, args);
                break;

            case LogLevel.Warning:
                logger.LogWarning(message, args);
                break;

            case LogLevel.Error:
                logger.LogError(message, args);
                break;

            case LogLevel.Critical:
                logger.LogCritical(message, args);
                break;

            case LogLevel.None:
                break;

            default:
                throw new NotImplementedException($"Level '{logLevel}' is not implemented!");
        }
    }

    /// <summary>
    /// Add a Log entry at Information level which automatically populates the function name
    /// https://stackoverflow.com/a/2652591/206852
    /// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute?redirectedfrom=MSDN&view=net-5.0
    public static void LogFunctionEnd(this ILogger logger, [CallerMemberName] string functionName = "")
    {
        logger.LogInformation("*** EXIT {0} ***", functionName);
    }

    /// <summary>
    /// Add a Log entry at Information level which automatically populates the function name
    /// https://stackoverflow.com/a/2652591/206852
    /// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute?redirectedfrom=MSDN&view=net-5.0
    public static void LogFunctionEnd(this ILogger logger, LogLevel logLevel, [CallerMemberName] string functionName = "")
    {
        string msg = "*** EXIT {0} ***";
        logger.LogAtDefinedLevel(msg, logLevel, functionName);
    }

    /// <summary>
    /// Add a Log entry at Information level which automatically populates the function name
    /// https://stackoverflow.com/a/2652591/206852
    /// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute?redirectedfrom=MSDN&view=net-5.0
    public static void LogFunctionStart(this ILogger logger, [CallerMemberName] string functionName = "")
    {
        logger.LogInformation("*** {0} ***", functionName);
    }

    /// <summary>
    /// Add a Log entry at Information level which automatically populates the function name
    /// https://stackoverflow.com/a/2652591/206852
    /// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute?redirectedfrom=MSDN&view=net-5.0
    public static void LogFunctionStart(this ILogger logger, LogLevel logLevel, [CallerMemberName] string functionName = "")
    {
        string msg = "*** {0} ***";
        logger.LogAtDefinedLevel(msg, logLevel, functionName);        
    }

    /// <summary>
    /// Add a Log entry at Information level which automatically populates the function name and passes the given args to the logentry
    /// https://stackoverflow.com/a/2652591/206852
    /// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute?redirectedfrom=MSDN&view=net-5.0
    /// </summary>
    /// <param name="functionName"></param>
    public static LogMessageBuilder LogFunctionStartWithArgs(this ILogger logger, [CallerMemberName] string functionName = "")
    {
        return new LogMessageBuilder("*** {0} ***", functionName, LogLevel.Information, true, logger.LogAtDefinedLevel);
    }

    /// <summary>
    /// Add a Log entry at Information level which automatically populates the function name and passes the given args to the logentry
    /// https://stackoverflow.com/a/2652591/206852
    /// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute?redirectedfrom=MSDN&view=net-5.0
    /// </summary>
    /// <param name="functionName"></param>
    public static LogMessageBuilder LogFunctionStartWithArgs(this ILogger logger, LogLevel logLevel, [CallerMemberName] string functionName = "")
    {
        return new LogMessageBuilder("*** {0} ***", functionName, logLevel, true, logger.LogAtDefinedLevel);
    }

    public static void LogWithFunctionName(this ILogger logger, string message, LogLevel logLevel, [CallerMemberName] string functionName = "")
    {
        logger.LogAtDefinedLevel($"{{0}}: {message}", logLevel, functionName);
    }

    public static LogMessageBuilder LogWithFunctionNameAndArgs(this ILogger logger, string message, LogLevel logLevel, [CallerMemberName] string functionName = "")
    {
        message = MoveArguments(message);
        return new LogMessageBuilder($"{{0}}: {message}", functionName, logLevel, false, logger.LogAtDefinedLevel);
    }

    private static string MoveArguments(string input)
    {
        StringBuilder output = new StringBuilder(input);
        List<int> argsToShift = new List<int>();
        Regex r = new Regex("\\{\\d\\}");
        foreach (Match m in r.Matches(input))
            argsToShift.Add(Convert.ToInt32(m.Value.Replace("{", "").Replace("}", "")));

        foreach(int i in argsToShift.OrderByDescending(v => v))
            output.Replace($"{{{i}}}", $"{{{(i + 1)}}}");

        return output.ToString();
    }

    //https://stackoverflow.com/a/67206700/206852
    public struct LogMessageBuilder
    {
        private readonly bool _isFunctionStart_End = false;
        private StringBuilder _message;
        private readonly string _functionName;
        private readonly LogLevel _logLevel;
        private readonly Action<string, LogLevel, object[]> _logAction;

        public LogMessageBuilder(string msg, string functionName, LogLevel logLevel, bool isFunctionStart_End, Action<string, LogLevel, object[]> logAction)
        {
            _isFunctionStart_End = isFunctionStart_End;
            _message = new StringBuilder(msg);
            _functionName = functionName;
            _logLevel = logLevel;
            _logAction = logAction;
        }

        public void Values(params object[] values)
        {
            object[] newValues = new object[values.Length + 1];
            newValues[0] = _functionName;

            for (int x = 0; x < values.Length; x++)
            {
                if (_isFunctionStart_End)
                    _message = _message.Replace($"{{{x}}}", $"{{{x}}}({{{(x + 1)}}}, )");

                newValues[(x + 1)] = values[x];
            }

            _message = _message.Replace(", )", ")");
            _logAction(_message.ToString(), _logLevel, newValues);
        }
    }
}
