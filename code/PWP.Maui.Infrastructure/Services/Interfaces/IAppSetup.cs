using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using PWP.Maui.Domain;

namespace PWP.Maui.Infrastructure.Services.Interfaces;

public interface IAppSetup
{
    void ConfigureDbContext(MauiAppBuilder builder);
    void ConfigureLogging(MauiAppBuilder builder);
    void ConfigureRuntimeValues(MauiAppBuilder builder, string dbFilename);
    void ConfigureWindowResizeHandler(ILifecycleBuilder events);
    void ConfigureWindowSizeAndPosition(object? sender, EventArgs e);
    void Exceptions_FirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs args);
    void Exceptions_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args);
    void Exceptions_UnhandledException(object? sender, UnhandledExceptionEventArgs args);
}
