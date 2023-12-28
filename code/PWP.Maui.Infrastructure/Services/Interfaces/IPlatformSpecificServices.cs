using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using System.Globalization;

namespace PWP.Maui.Infrastructure.Services.Interfaces;

public interface IPlatformSpecificServices
{
    string AssemblyFileVersion { get; }
    string AssemblyInformationalVersion { get; }
    Page? MainPage { get; set; }
    void ChangeCulture(CultureInfo newCulture);
    void ChangeLogLevel(string newLevel);
    void ChangeLogLevel(LogLevel newLevel);
    string GetFileSystemAppDataDirectory();
    void InitializeDb();
    void InitializeTranslations();
    bool IsDebug();
    bool IsMobile();
    ILogger MemoryLoggerCreate(Type targetType, LogLevel logLevel);
    IList<string> MemoryLoggerGetLogs(Type targetType);
    string MemoryLoggerGetTargetName(Type targetType);
    void MemoryLoggerRemove(Type targetType);
    void SwitchNLogInternalLog(bool on);
}
