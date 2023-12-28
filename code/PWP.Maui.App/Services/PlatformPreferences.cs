using PWP.Maui.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace PWP.Maui.App.Services;

public class PlatformPreferences : IPlatformPreferences
{
    public void Clear()
    {
        Preferences.Clear();
    }

    public void Clear(string? sharedName)
    {
        Preferences.Clear(sharedName);
    }

    public bool ContainsKey(string key)
    {
        return Preferences.ContainsKey(key);
    }

    public bool ContainsKey(string key, string? sharedName)
    {
        return Preferences.ContainsKey(key, sharedName);
    }

    public bool Get(string key, bool defaultValue)
    {
        return Preferences.Get(key, defaultValue);
    }

    public bool Get(string key, bool defaultValue, string? sharedName)
    {
        return Preferences.Get(key, defaultValue, sharedName);
    }

    public DateTime Get(string key, DateTime defaultValue)
    {
        return Preferences.Get(key, defaultValue);
    }

    public DateTime Get(string key, DateTime defaultValue, string? sharedName)
    {
        throw new NotImplementedException();
    }

    public double Get(string key, double defaultValue)
    {
        return Preferences.Get(key, defaultValue);
    }

    public double Get(string key, double defaultValue, string? sharedName)
    {
        return Preferences.Get(key, defaultValue, sharedName);
    }

    public int Get(string key, int defaultValue)
    {
        return Preferences.Get(key, defaultValue);
    }

    public int Get(string key, int defaultValue, string? sharedName)
    {
        return Preferences.Get(key, defaultValue, sharedName);
    }

    public LogLevel Get(string key, LogLevel defaultValue)
    {
        string value = Preferences.Get(key, defaultValue.ToString());
        return Enum.Parse<LogLevel>(value);
    }

    public LogLevel Get(string key, LogLevel defaultValue, string? sharedName)
    {
        string value = Preferences.Get(key, defaultValue.ToString(), sharedName);
        return Enum.Parse<LogLevel>(value);
    }

    public long Get(string key, long defaultValue)
    {
        return Preferences.Get(key, defaultValue);
    }

    public long Get(string key, long defaultValue, string? sharedName)
    {
        return Preferences.Get(key, defaultValue, sharedName);
    }

    public float Get(string key, float defaultValue)
    {
        return Preferences.Get(key, defaultValue);
    }

    public float Get(string key, float defaultValue, string? sharedName)
    {
        return Preferences.Get(key, defaultValue, sharedName);
    }

    public string Get(string key, string defaultValue)
    {
        return Preferences.Get(key, defaultValue);
    }

    public string Get(string key, string defaultValue, string? sharedName)
    {
        return Preferences.Get(key, defaultValue, sharedName);
    }

    public void Remove(string key)
    {
        Preferences.Remove(key);
    }

    public void Remove(string key, string? sharedName)
    {
        Preferences.Remove(key, sharedName);
    }

    public void Set(string key, bool value)
    {
        Preferences.Set(key, value);
    }

    public void Set(string key, bool value, string? sharedName)
    {
        Preferences.Set(key, value, sharedName);
    }

    public void Set(string key, DateTime value)
    {
        Preferences.Set(key, value);
    }

    public void Set(string key, DateTime value, string? sharedName)
    {
        Preferences.Set(key, value, sharedName);
    }

    public void Set(string key, double value)
    {
        Preferences.Set(key, value);
    }

    public void Set(string key, double value, string? sharedName)
    {
        Preferences.Set(key, value, sharedName);
    }

    public void Set(string key, int value)
    {
        Preferences.Set(key, value);
    }

    public void Set(string key, int value, string? sharedName)
    {
        Preferences.Set(key, value, sharedName);
    }

    public void Set(string key, LogLevel value)
    {
        Preferences.Set(key, value.ToString());
    }

    public void Set(string key, LogLevel value, string? sharedName)
    {
        Preferences.Set(key, value.ToString(), sharedName);
    }

    public void Set(string key, long value)
    {
        Preferences.Set(key, value);
    }

    public void Set(string key, long value, string? sharedName)
    {
        Preferences.Set(key, value, sharedName);
    }

    public void Set(string key, float value)
    {
        Preferences.Set(key, value);
    }

    public void Set(string key, float value, string? sharedName)
    {
        Preferences.Set(key, value, sharedName);
    }

    public void Set(string key, string value)
    {
        Preferences.Set(key, value);
    }

    public void Set(string key, string value, string? sharedName)
    {
        Preferences.Set(key, value, sharedName);
    }
}
