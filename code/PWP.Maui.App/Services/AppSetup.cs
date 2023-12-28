using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using NLog.Extensions.Logging;
using NLog.Filters;
using NLog.Targets;
using NLog;
using NLog.Layouts;
using PWP.Maui.Domain;
using PWP.Maui.Infrastructure.Data;
using PWP.Maui.Infrastructure.Services.Interfaces;
using PWP.Maui.Utils;
using PWP.Maui.Utils.Extensions;
using System.Text;
using PWP.Maui.Infrastructure.Services.Implementations;

namespace PWP.Maui.App.Services;

public class AppSetup : IAppSetup
{
    private ILogger<AppSetup>? _logger;
    private IPlatformPreferences _platformPreferences;
    private RuntimeValues _runtimeValues;
    private IServiceProvider _serviceProvider;

#if WINDOWS
    //https://learn.microsoft.com/en-us/answers/questions/1165153/in-wpf-i-want-to-handle-the-start-and-end-of-resiz
    private static Microsoft.UI.Xaml.DispatcherTimer _windowResizedtimer = new Microsoft.UI.Xaml.DispatcherTimer
    {
        Interval = new TimeSpan(0, 0, 0, 0, 500)
    };

    private static Microsoft.UI.Xaml.WindowSizeChangedEventArgs? _windowSizeChangedEventArgs;
#endif

    #region Constructor
    public AppSetup(IServiceProvider serviceProvider, IPlatformPreferences platformPreferences, RuntimeValues runtimeValues)
    {
        _platformPreferences = platformPreferences;
        _runtimeValues = runtimeValues;
        _serviceProvider = serviceProvider;

        if (_logger == null)
        {
            ILoggerFactory? lf = _serviceProvider.GetService<ILoggerFactory>();
            if (lf != null)
                _logger = lf.CreateLogger<AppSetup>();
        }
    }
    #endregion

    #region ConfigureDbContext
    public void ConfigureDbContext(MauiAppBuilder builder)
    {
        Guard.ThrowIfNull(builder);
        Guard.ThrowIfNull(_runtimeValues);

        if (!_runtimeValues.Configured)
            throw new Exception("RuntimeValues must be configured before configuring Logging!");

        builder.Services
        //https://learn.microsoft.com/en-gb/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli
            .AddDbContext<IPWPMauiDataContext, PWPMauiDataContext>((sp, options) =>
            {
                //https://learn.microsoft.com/en-gb/ef/core/providers/sqlite/?tabs=dotnet-core-cli
                options.UseSqlite(_runtimeValues.DbConnectionString, (sqliteOpts) =>
                {
                    sqliteOpts.CommandTimeout(10);
                    sqliteOpts.MigrationsAssembly(typeof(IPWPMauiDataContext).Assembly.FullName);
                });
            });
    }
    #endregion

    #region ConfigureLogging
    public void ConfigureLogging(MauiAppBuilder builder)
    {
        Guard.ThrowIfNull(_runtimeValues);
        if (!_runtimeValues.Configured)
            throw new Exception("RuntimeValues must be configured before configuring Logging!");

        builder.Logging.ClearProviders();
        builder.Logging.AddNLog();

        //https://github.com/NLog/NLog/wiki/Fluent-Configuration-API
        //https://stackoverflow.com/questions/49337760/how-to-change-the-nlog-layout-at-run-time
        LogManager.Setup().RegisterMauiLog()
            .LoadConfiguration(builder =>
            {
                SimpleLayout layout = new SimpleLayout(AppConstants.Logging.DEFAULT_LOG_FORMAT);
                SimpleLayout filename = new SimpleLayout(_runtimeValues!.LogFullpath);

                builder.ForLogger("Microsoft.AspNetCore.Routing.*").WriteToNil(NLog.LogLevel.Warn);
                builder.ForLogger("Microsoft.AspNetCore.Components.*").WriteToNil(NLog.LogLevel.Warn);
                builder.ForLogger("Microsoft.EntityFrameworkCore.Database.Connection").WriteToNil(NLog.LogLevel.Warn);
                builder.ForLogger("Microsoft.EntityFrameworkCore.ChangeTracking").WriteToNil(NLog.LogLevel.Warn);
                builder.ForLogger("MudBlazor.*").WriteToNil(NLog.LogLevel.Warn);
                //builder.ForLogger().FilterMinLevel(NLog.LogLevel.FromString(_runtimeValues.LogLevel.ToString())).WriteToFile(filename, layout, Encoding.UTF8, NLog.Targets.LineEndingMode.Default, true, false, 262144, 3, 5);
                //builder.ForLogger().FilterMinLevel(NLog.LogLevel.FromString(_runtimeValues.LogLevel.ToString())).WriteToMauiLog(layout);
            })
        .GetCurrentClassLogger();

        FileTarget fileTarget = new FileTarget(AppConstants.Logging.MAIN_LOG_NAME);
        fileTarget.Layout = AppConstants.Logging.DEFAULT_LOG_FORMAT;
        fileTarget.FileName = _runtimeValues!.LogFullpath;
        fileTarget.Encoding = Encoding.UTF8;
        fileTarget.LineEnding = LineEndingMode.Default;
        fileTarget.KeepFileOpen = true;
        fileTarget.ConcurrentWrites = false;
        fileTarget.ArchiveAboveSize = 262144;
        fileTarget.MaxArchiveFiles = 3;
        fileTarget.MaxArchiveDays = 2;

        //https://github.com/NLog/NLog/wiki/Filtering-log-messages
        NLog.Config.LoggingRule rule = new NLog.Config.LoggingRule(AppConstants.Logging.MAIN_LOG_RULE_NAME);
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

        for (int x = (int)_runtimeValues.LogLevel; x <= 5; x++)
            rule.EnableLoggingForLevel(NLog.LogLevel.FromOrdinal(x));

        rule.Targets.Add(fileTarget);
        rule.LoggerNamePattern = "*";
        LogManager.Configuration.AddRule(rule);
        LogManager.ReconfigExistingLoggers();

        ServiceProvider sp = builder.Services.BuildServiceProvider();
        ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<AppSetup>();
        _logger.LogInformation("Logging configured");
        _logger.LogDebug($"Logs folder: {_runtimeValues.LogFolder}");
    }
    #endregion

    #region ConfigureRuntimeValues
    public void ConfigureRuntimeValues(MauiAppBuilder builder, string dbFilename)
    {
        ServiceProvider sp = builder.Services.BuildServiceProvider();
        _platformPreferences = sp.GetRequiredService<IPlatformPreferences>();
        _runtimeValues.DbFilename = dbFilename;
        _runtimeValues.LogLevel = _platformPreferences.Get(AppConstants.PreferenceKeys.LOG_LEVEL, Microsoft.Extensions.Logging.LogLevel.Debug);
        _runtimeValues.NLogInternalLogOn = _platformPreferences.Get(AppConstants.PreferenceKeys.LOG_INTERNAL_LOG_ON, false);
        _runtimeValues.Configured = true;
    }
    #endregion

    #region ConfigureWindowResizeHandler
    public void ConfigureWindowResizeHandler(ILifecycleBuilder events)
    {
        //window sizing not available on Mac
        //https://github.com/dotnet/maui/pull/4942
        //https://github.com/dotnet/maui/issues/9704
#if WINDOWS
        events.AddWindows(windows => 
            windows.OnWindowCreated(window =>
            {
                window.SizeChanged += OnSizeChanged;
            }));

        _windowResizedtimer.Tick += WindowResized_Tick;
#endif
    }
    #endregion

    #region ConfigureWindowSizeAndPosition
    public async void ConfigureWindowSizeAndPosition(object? sender, EventArgs e)
    {
        //https://stackoverflow.com/a/74242566/206852
        if (OperatingSystem.IsWindows())
        {
            var disp = DeviceDisplay.Current.MainDisplayInfo;

            double defaultWidth = Math.Min(_platformPreferences.Get(AppConstants.PreferenceKeys.SIZE_WIDTH, 1600D), disp.Width);
            double defaultHeight = Math.Min(_platformPreferences.Get(AppConstants.PreferenceKeys.SIZE_HEIGHT, 1000D), disp.Height);

            var window = sender as Window;
            if (window != null)
            {
                // change window size.
                window.Width = defaultWidth;
                window.Height = defaultHeight;

                // give it some time to complete window resizing task.
                await window.Dispatcher.DispatchAsync(() => { });

                // move to screen center
                window.X = (disp.Width / disp.Density - window.Width) / 2;
                window.Y = (disp.Height / disp.Density - window.Height) / 2;
            }
        }
    }
    #endregion

    #region Exceptions_FirstChanceException
    //https://github.com/dotnet/maui/discussions/653#discussioncomment-2445428
    //https://github.com/xamarin/xamarin-macios/issues/15252#issuecomment-1154200053
    //https://github.com/NLog/NLog.Targets.MauiLog/wiki/Logging-Unhandled-Exceptions
    //Note that iOS and MacCatalyst have their extra unhandled exception options configured in their respective Program.cs files
    public void Exceptions_FirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs args)
    {
        //https://github.com/dotnet/maui/discussions/653#discussioncomment-2445428
        _logger!.LogError(args.Exception, "FirstChanceException");
    }
    #endregion

    #region Exceptions_UnobservedTaskException
    public void Exceptions_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args)
    {
        _logger!.LogError(args.Exception, "UnobservedTaskException");
    }
    #endregion

    #region Exceptions_UnhandledException
    public void Exceptions_UnhandledException(object? sender, UnhandledExceptionEventArgs args)
    {
        _logger!.LogError((Exception)args.ExceptionObject, "UnhandledException");
    }
    #endregion

#if WINDOWS
    private static void OnSizeChanged(object? sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs args)
    {
        _windowSizeChangedEventArgs = args;

        //reset the timer if it's already running
        if (_windowResizedtimer.IsEnabled)
            _windowResizedtimer.Stop();

        _windowResizedtimer.Start();

        ILifecycleEventService service = MauiWinUIApplication.Current.Services.GetRequiredService<ILifecycleEventService>();
        service.InvokeEvents(nameof(Microsoft.UI.Xaml.Window.SizeChanged));
    }

    private static void WindowResized_Tick(object? sender, object e)
    {
        _logger!.LogWithFunctionNameAndArgs("Window resize finished: storing Width: {0}, Height: {1}", Microsoft.Extensions.Logging.LogLevel.Debug).Values(_windowSizeChangedEventArgs!.Size.Width, _windowSizeChangedEventArgs!.Size.Height);
        
        //https://learn.microsoft.com/en-us/answers/questions/1165153/in-wpf-i-want-to-handle-the-start-and-end-of-resiz
        _windowResizedtimer.Stop();

        if (_platformPreferences != null)
        {
            _platformPreferences.Set(PWPConstants.PreferenceKeys.SIZE_WIDTH, _windowSizeChangedEventArgs!.Size.Width);
            _platformPreferences.Set(PWPConstants.PreferenceKeys.SIZE_HEIGHT, _windowSizeChangedEventArgs!.Size.Height);
        }
    }
#endif
}
