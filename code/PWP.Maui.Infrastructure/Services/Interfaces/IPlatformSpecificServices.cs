using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using System.Globalization;

namespace PWP.Maui.Infrastructure.Services.Interfaces;

public interface IPlatformSpecificServices
{
    string AssemblyFileVersion { get; }
    string AssemblyInformationalVersion { get; }
    void ChangeCulture(Type mainPageType, CultureInfo newCulture);
    void ChangeLogLevel(string newLevel);
    void ChangeLogLevel(LogLevel newLevel);
    string GetFileSystemAppDataDirectory();
    bool IsDebug();
    bool IsMobile();
    ILogger MemoryLoggerCreate(Type targetType, LogLevel logLevel);
    IList<string> MemoryLoggerGetLogs(Type targetType);
    string MemoryLoggerGetTargetName(Type targetType);
    void MemoryLoggerRemove(Type targetType);
    void SetupLogging(MauiAppBuilder builder);
    void SwitchNLogInternalLog(bool on);
}
