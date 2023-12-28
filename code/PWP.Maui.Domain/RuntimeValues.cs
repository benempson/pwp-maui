using Microsoft.Extensions.Logging;

namespace PWP.Maui.Domain;

public class RuntimeValues
{
    private string _dbFilename = "PWPMaui_Db.db3";

    /// <summary>
    /// Whether or not the RuntimeValue have been configured
    /// </summary>
    public bool Configured { get; set; } = false;

    /// <summary>
    /// The Sqlite Db filename
    /// </summary>
    public string DbFilename
    {
        get { return _dbFilename; }
        set { SetDbPaths(value); }
    }

    /// <summary>
    /// The Sqlite connection string
    /// </summary>
    public string? DbConnectionString { get; private set; }

    /// <summary>
    /// The folder containing the Sqlite Db files
    /// </summary>
    public string DbFolder { get;private set; }

    /// <summary>
    /// The full path to the Sqlite database
    /// </summary>
    public string? DbFullPath { get; private set; }

    /// <summary>
    /// Whether or not the Db has been initialized
    /// </summary>
    public bool DbInitialized { get; set; } = false;

    /// <summary>
    /// The local application data folder (platform specific)
    /// </summary>
    public string LocalAppDataFolder { get; private set; }

    /// <summary>
    /// The path to the folder containing the application log files
    /// </summary>
    public string LogFolder { get; private set; }

    /// <summary>
    /// The filename for the application log file
    /// </summary>
    public readonly string LogFilename = "log.log";

    /// <summary>
    /// The full path for the application log file
    /// </summary>
    public string LogFullpath { get; private set; }

    public LogLevel LogLevel { get; set; } = LogLevel.Debug;

    public bool NavOpen { get; set; } = false;

    /// <summary>
    /// The filename for the NLog internal log file
    /// </summary>
    public readonly string NLogInternalLogFilename = "nlog-internal.log";

    /// <summary>
    /// Whether or not the NLog internal log is switched on
    /// </summary>
    public bool NLogInternalLogOn {  get; set; }

    /// <summary>
    /// The full path to the NLog internal log file
    /// </summary>
    public string NLogInternalLogFullpath { get; private set; }
    
    public RuntimeValues(string localAppDataFolder)
    {
        LocalAppDataFolder = localAppDataFolder;

        DbFolder = localAppDataFolder;
        SetDbPaths(_dbFilename);
        
        LogFolder = Path.Combine(LocalAppDataFolder, "Logs");
        LogFullpath = Path.Combine(LogFolder, LogFilename);
        NLogInternalLogFullpath = Path.Combine(LogFolder, NLogInternalLogFilename);
    }

    private void SetDbPaths(string dbFilename)
    {
        _dbFilename = dbFilename;
        DbFullPath = Path.Combine(LocalAppDataFolder!, DbFilename);
        DbConnectionString = $"Data Source={DbFullPath}";
    }
}