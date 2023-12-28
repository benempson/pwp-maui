using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using NLog;
using NLog.Extensions.Logging;
using NLog.Filters;
using NLog.Layouts;
using NLog.Targets;
using PWP.Maui.Domain;
using PWP.Maui.Infrastructure.Data;
using PWP.Maui.Infrastructure.Services.Interfaces;
using PWP.Maui.Utils;
using PWP.Maui.Utils.Extensions;
using System.Globalization;
using System.Text;

namespace PWP.Maui.App.Services;

public class PlatformSpecificServices : IPlatformSpecificServices
{
    private IPWPMauiDataContext _dataContext;
    private ILogger<PlatformSpecificServices> _logger;
    private ILoggerFactory _loggerFactory;
    private IPlatformPreferences _platformPreferences;
    private PWPRuntimeValues _pwpRuntimeValues;
    private ITranslationService _translationService;

    public PlatformSpecificServices(IPWPMauiDataContext dataContext, ILogger<PlatformSpecificServices> logger, ILoggerFactory loggerFactory, IPlatformPreferences platformPreferences, PWPRuntimeValues pwpRuntimeValues, ITranslationService translationServices)
    {
        _dataContext = dataContext;
        _logger = logger;
        _logger.LogFunctionStart();

        _loggerFactory = loggerFactory;
        _platformPreferences = platformPreferences;
        _pwpRuntimeValues = pwpRuntimeValues;
        _translationService = translationServices;
    }

    public string AssemblyFileVersion { get { return ThisAssembly.AssemblyFileVersion; } }

    public string AssemblyInformationalVersion { get { return ThisAssembly.AssemblyInformationalVersion; } }

    public void ChangeCulture(Type mainPageType, CultureInfo newCulture)
    {
        _logger.LogFunctionStart();
        
        //https://stackoverflow.com/questions/76555754/how-to-reload-or-update-ui-in-net-maui-when-culture-has-been-changed
        //Any change to the Culture properties causes "Process terminated due to "Infinite recursion during resource lookup within System.Private.CoreLib.  This may be a bug in System.Private.CoreLib, or potentially in certain extensibility points such as assembly resolve events or CultureInfo names.  Resource name: IO_FileName_Name"
        //Errors in debug output prior to final crash: [monodroid-assembly] open_from_bundles: failed to load assembly en-GB/System.Private.CoreLib.resources.dll
        if (Application.Current != null)
        {
            Microsoft.Extensions.Logging.ILogger mpLogger = _loggerFactory.CreateLogger(mainPageType);
            ContentPage newMainPage = (Activator.CreateInstance(mainPageType, new object[] { _dataContext, mpLogger, _platformPreferences, _pwpRuntimeValues, _translationService }) as ContentPage)!;
            Application.Current.MainPage = newMainPage!;
        }

        //(Application.Current as App).MainPage.Dispatcher.Dispatch(() =>
        //{
        //    Thread.CurrentThread.CurrentCulture = newCulture;
        //    Thread.CurrentThread.CurrentUICulture = newCulture;
        //    CultureInfo.DefaultThreadCurrentCulture = newCulture;
        //    CultureInfo.DefaultThreadCurrentUICulture = newCulture;
        //});

        _translationService.LoadTranslations();
    }

    public void ChangeLogLevel(string newLevel)
    {
        _logger.LogFunctionStartWithArgs().Values(newLevel);
        _platformPreferences.Set(PWPConstants.PreferenceKeys.LOG_LEVEL, newLevel);
        Microsoft.Extensions.Logging.LogLevel logLevel = Enum.Parse<Microsoft.Extensions.Logging.LogLevel>(newLevel);
        ChangeLogLevel(logLevel);
    }

    public void ChangeLogLevel(Microsoft.Extensions.Logging.LogLevel newLevel)
    {
        NLog.LogLevel newNlogLevel = NLog.LogLevel.FromOrdinal((int)newLevel);

        foreach (var rule in LogManager.Configuration.LoggingRules)
        {
            if (rule.LoggerNamePattern.Equals("*"))
                rule.SetLoggingLevels(newNlogLevel, NLog.LogLevel.Fatal);
        }

        LogManager.ReconfigExistingLoggers();
        _pwpRuntimeValues.LogLevel = newLevel;
    }

    public string GetFileSystemAppDataDirectory()
    {
        return FileSystem.Current.AppDataDirectory;
    }

    /// <summary>
    /// Whether or not the DEBUG PreProcessor directive is present
    /// </summary>
    /// <returns></returns>
    public bool IsDebug()
    {
#if DEBUG
        return true;
#else
      return false;
#endif
    }

    public bool IsMobile()
    {
#if ANDROID
    return true;
#elif IOS
    return true;
#endif
#pragma warning disable CS0162 // Unreachable code detected
        return false;
#pragma warning restore CS0162 // Unreachable code detected
    }

    /// <summary>
    /// Creates a Memory log target with the given log target name
    /// https://github.com/NLog/NLog/wiki/Memory-target
    /// </summary>
    /// <param name="targetType">The Type of Memory Logger to create</param>
    /// <returns></returns>
    public Microsoft.Extensions.Logging.ILogger MemoryLoggerCreate(Type targetType, Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        string mtName = MemoryLoggerGetTargetName(targetType);
        MemoryTarget memoryTarget = new MemoryTarget(mtName);
        memoryTarget.Layout = PWPConstants.Logging.DEFAULT_LOG_FORMAT;
        NLog.Config.LoggingRule rule = new NLog.Config.LoggingRule(mtName);
        for (int x = (int)logLevel; x <= 5; x++)
            rule.EnableLoggingForLevel(NLog.LogLevel.FromOrdinal(x));

        rule.Targets.Add(memoryTarget);
        rule.LoggerNamePattern = "*";
        LogManager.Configuration.AddRule(rule);
        LogManager.ReconfigExistingLoggers();
        return _loggerFactory.CreateLogger(mtName);
        //return LogManager.GetLogger(targetType.Name);
    }

    public IList<string> MemoryLoggerGetLogs(Type targetType)
    {
        MemoryTarget? mt = LogManager.Configuration.AllTargets.SingleOrDefault(t => !string.IsNullOrWhiteSpace(t.Name) && t.Name.Equals(MemoryLoggerGetTargetName(targetType))) as MemoryTarget;
        Guard.ThrowIfNull(mt);
        return mt.Logs;
    }

    public string MemoryLoggerGetTargetName(Type targetType)
    {
        return $"{targetType.FullName}:ML";
    }

    public void MemoryLoggerRemove(Type targetType)
    {
        LogManager.Configuration.RemoveTarget(MemoryLoggerGetTargetName(targetType));
        NLog.Config.LoggingRule? ruleToRemove = LogManager.Configuration.LoggingRules.SingleOrDefault(r => r.RuleName != null && r.RuleName.Equals(MemoryLoggerGetTargetName(targetType)));
        if (ruleToRemove != null)
            LogManager.Configuration.LoggingRules.Remove(ruleToRemove);

        LogManager.ReconfigExistingLoggers();
    }

    public void SetupLogging(MauiAppBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddNLog();

        Guard.ThrowIfNull(_pwpRuntimeValues);

        //https://github.com/NLog/NLog/wiki/Fluent-Configuration-API
        //https://stackoverflow.com/questions/49337760/how-to-change-the-nlog-layout-at-run-time
        LogManager.Setup().RegisterMauiLog()
            .LoadConfiguration(builder =>
            {
                SimpleLayout layout = new SimpleLayout(PWPConstants.Logging.DEFAULT_LOG_FORMAT);
                SimpleLayout filename = new SimpleLayout(_pwpRuntimeValues!.LogFullpath);

                builder.ForLogger("Microsoft.AspNetCore.Routing.*").WriteToNil(NLog.LogLevel.Warn);
                builder.ForLogger("Microsoft.AspNetCore.Components.*").WriteToNil(NLog.LogLevel.Warn);
                builder.ForLogger("Microsoft.EntityFrameworkCore.Database.Connection").WriteToNil(NLog.LogLevel.Warn);
                builder.ForLogger("Microsoft.EntityFrameworkCore.ChangeTracking").WriteToNil(NLog.LogLevel.Warn);
                builder.ForLogger("MudBlazor.*").WriteToNil(NLog.LogLevel.Warn);
                //builder.ForLogger().FilterMinLevel(NLog.LogLevel.FromString(_runtimeValues.LogLevel.ToString())).WriteToFile(filename, layout, Encoding.UTF8, NLog.Targets.LineEndingMode.Default, true, false, 262144, 3, 5);
                //builder.ForLogger().FilterMinLevel(NLog.LogLevel.FromString(_runtimeValues.LogLevel.ToString())).WriteToMauiLog(layout);
            })
        .GetCurrentClassLogger();

        FileTarget fileTarget = new FileTarget(PWPConstants.Logging.MAIN_LOG_NAME);
        fileTarget.Layout = PWPConstants.Logging.DEFAULT_LOG_FORMAT;
        fileTarget.FileName = _pwpRuntimeValues!.LogFullpath;
        fileTarget.Encoding = Encoding.UTF8;
        fileTarget.LineEnding = LineEndingMode.Default;
        fileTarget.KeepFileOpen = true;
        fileTarget.ConcurrentWrites = false;
        fileTarget.ArchiveAboveSize = 262144;
        fileTarget.MaxArchiveFiles = 3;
        fileTarget.MaxArchiveDays = 2;

        //https://github.com/NLog/NLog/wiki/Filtering-log-messages
        NLog.Config.LoggingRule rule = new NLog.Config.LoggingRule(PWPConstants.Logging.MAIN_LOG_RULE_NAME);
        rule.FilterDefaultAction = FilterResult.Log;
        rule.Filters.Add(new WhenMethodFilter((logEventInfo) =>
        {
            if (logEventInfo.LoggerName.Equals("Microsoft.EntityFrameworkCore.Database.Command"))
            {
                if (logEventInfo.Message.Contains("Creating DbCommand for ") ||
                    logEventInfo.Message.Contains("Created DbCommand for ") ||
                    logEventInfo.Message.Contains("Initialized DbCommand for ") ||
                    logEventInfo.Message.Contains("Closing data reader to '") ||
                    (logEventInfo.Message.Contains("A data reader for '") && logEventInfo.Message.Contains("is being disposed after spending")))
                    return FilterResult.Ignore;
            }
            else if (logEventInfo.LoggerName.Equals("Microsoft.EntityFrameworkCore.Query"))
            {
                if (logEventInfo.Message.Contains("Compiling query expression"))
                    return FilterResult.Ignore;
            }
            else if (logEventInfo.LoggerName.Equals("Microsoft.EntityFrameworkCore.Infrastructure"))
            {
                if (logEventInfo.Message.Contains("An 'IServiceProvider' was created for internal use by Entity Framework"))
                    return FilterResult.Ignore;
            }

            return FilterResult.Log;
        }));

        for (int x = (int)_pwpRuntimeValues.LogLevel; x <= 5; x++)
            rule.EnableLoggingForLevel(NLog.LogLevel.FromOrdinal(x));

        rule.Targets.Add(fileTarget);
        rule.LoggerNamePattern = "*";
        LogManager.Configuration.AddRule(rule);
        LogManager.ReconfigExistingLoggers();
    }

    public void SwitchNLogInternalLog(bool on)
    {
        _logger.LogFunctionStartWithArgs().Values(on);
        _platformPreferences.Set(PWPConstants.PreferenceKeys.LOG_INTERNAL_LOG_ON, on);

        if (on)
        {
            NLog.Common.InternalLogger.LogFile = _pwpRuntimeValues.NLogInternalLogFullpath;
            NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Debug;
        }
        else
        {
            NLog.Common.InternalLogger.LogFile = null;
        }

        LogManager.ReconfigExistingLoggers();
    }
}
