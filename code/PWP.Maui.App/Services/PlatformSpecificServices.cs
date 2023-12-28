using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
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
    private int _dbInitializationAttempts = 0;
    private Exception? _dbInitializationError;
    private ILogger<PlatformSpecificServices> _logger;
    private ILoggerFactory _loggerFactory;
    private IPlatformPreferences _platformPreferences;
    private RuntimeValues _runtimeValues;
    private IServiceProvider _serviceProvider;
    private ITranslationService _translationService;

    public PlatformSpecificServices(IServiceProvider serviceProvider, IPWPMauiDataContext dataContext, ILogger<PlatformSpecificServices> logger, ILoggerFactory loggerFactory, IPlatformPreferences platformPreferences, RuntimeValues runtimeValues, ITranslationService translationServices)
    {
        _dataContext = dataContext;
        _logger = logger;
        _logger.LogFunctionStart();

        _loggerFactory = loggerFactory;
        _platformPreferences = platformPreferences;
        _runtimeValues = runtimeValues;
        _serviceProvider = serviceProvider;
        _translationService = translationServices;
    }

    #region Properties
    public string AssemblyFileVersion { get { return ThisAssembly.AssemblyFileVersion; } }

    public string AssemblyInformationalVersion { get { return ThisAssembly.AssemblyInformationalVersion; } }

    public Page? MainPage { get; set; }

    #endregion

    #region ChangeCulture
    public void ChangeCulture(CultureInfo newCulture)
    {
        _logger.LogFunctionStartWithArgs().Values(newCulture.Name);
        Guard.ThrowIfNull(MainPage);
        
        if (Application.Current != null)
        {
            //Microsoft.Extensions.Logging.ILogger mpLogger = _loggerFactory.CreateLogger(MainPage.GetType());
            //Page? newMainPage = (Page?)Activator.CreateInstance(MainPage.GetType(), new object[] { mpLogger, _serviceProvider.GetRequiredService<IPlatformSpecificServices>() });
            Application.Current.MainPage = MainPage;
        }

        //https://stackoverflow.com/questions/76555754/how-to-reload-or-update-ui-in-net-maui-when-culture-has-been-changed
        //Any change to the Culture properties causes "Process terminated due to "Infinite recursion during resource lookup within System.Private.CoreLib.  This may be a bug in System.Private.CoreLib, or potentially in certain extensibility points such as assembly resolve events or CultureInfo names.  Resource name: IO_FileName_Name"
        //Errors in debug output prior to final crash: [monodroid-assembly] open_from_bundles: failed to load assembly en-GB/System.Private.CoreLib.resources.dll
        //(Application.Current as App).MainPage.Dispatcher.Dispatch(() =>
        //{
        //    Thread.CurrentThread.CurrentCulture = newCulture;
        //    Thread.CurrentThread.CurrentUICulture = newCulture;
        //    CultureInfo.DefaultThreadCurrentCulture = newCulture;
        //    CultureInfo.DefaultThreadCurrentUICulture = newCulture;
        //});

        _translationService.LoadTranslations();
    }
    #endregion

    #region ChangeLogLevel
    public void ChangeLogLevel(string newLevel)
    {
        _logger.LogFunctionStartWithArgs().Values(newLevel);
        _platformPreferences.Set(AppConstants.PreferenceKeys.LOG_LEVEL, newLevel);
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
        _runtimeValues.LogLevel = newLevel;
    }
    #endregion

    #region GetFileSystemAppDataDirectory
    public string GetFileSystemAppDataDirectory()
    {
        return FileSystem.Current.AppDataDirectory;
    }
    #endregion

    #region InitializeDb
    public void InitializeDb()
    {
        _logger.LogFunctionStart();

        _dbInitializationAttempts++;
        if (_dbInitializationAttempts >= 3)
        {
            _logger.LogCritical(_dbInitializationError!, "Unable to Initialize Db");
            throw new Exception("Unable to Initialize Db");
        }

        try
        {
            _dataContext.Database.Migrate();
            _dataContext.Initialize();
            _runtimeValues.DbInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "");

            //if we get an error initializaing the Db, then simply delete it and start again
            _dbInitializationError = ex;
            _dataContext.Database.CloseConnection();
            _dataContext.Database.EnsureDeleted();
            InitializeDb();
        }

        _logger.LogFunctionEnd();
    }
    #endregion

    #region InitializeTranslations
    public void InitializeTranslations()
    {
        _translationService.LoadTranslations();
    }
    #endregion

    #region IsDebug
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
    #endregion

    #region IsMobile
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
    #endregion

    #region MemoryLoggerCreate
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
        memoryTarget.Layout = AppConstants.Logging.DEFAULT_LOG_FORMAT;
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
    #endregion

    #region MemoryLoggerGetLogs
    public IList<string> MemoryLoggerGetLogs(Type targetType)
    {
        MemoryTarget? mt = LogManager.Configuration.AllTargets.SingleOrDefault(t => !string.IsNullOrWhiteSpace(t.Name) && t.Name.Equals(MemoryLoggerGetTargetName(targetType))) as MemoryTarget;
        Guard.ThrowIfNull(mt);
        return mt.Logs;
    }
    #endregion

    #region MemoryLoggerGetTargetName
    public string MemoryLoggerGetTargetName(Type targetType)
    {
        return $"{targetType.FullName}:ML";
    }
    #endregion

    #region MemoryLoggerRemove
    public void MemoryLoggerRemove(Type targetType)
    {
        LogManager.Configuration.RemoveTarget(MemoryLoggerGetTargetName(targetType));
        NLog.Config.LoggingRule? ruleToRemove = LogManager.Configuration.LoggingRules.SingleOrDefault(r => r.RuleName != null && r.RuleName.Equals(MemoryLoggerGetTargetName(targetType)));
        if (ruleToRemove != null)
            LogManager.Configuration.LoggingRules.Remove(ruleToRemove);

        LogManager.ReconfigExistingLoggers();
    }
    #endregion

    #region SwitchNLogInternalLog
    public void SwitchNLogInternalLog(bool on)
    {
        _logger.LogFunctionStartWithArgs().Values(on);
        _platformPreferences.Set(AppConstants.PreferenceKeys.LOG_INTERNAL_LOG_ON, on);

        if (on)
        {
            NLog.Common.InternalLogger.LogFile = _runtimeValues.NLogInternalLogFullpath;
            NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Debug;
        }
        else
        {
            NLog.Common.InternalLogger.LogFile = null;
        }

        LogManager.ReconfigExistingLoggers();
    }
    #endregion
}
