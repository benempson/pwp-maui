namespace PWP.Maui.Domain;

public class PWPConstants
{
    public struct CultureRefs
    {
        public static string GetFullCulture(string inputName)
        {
            switch (inputName)
            {
                case "en":
                    return "en-US";

                case "es":
                    return "es-ES";

                default:
                    if (inputName.Length == 2)
                        throw new NotImplementedException($"Culture {inputName} is not implemented!");
                    else
                        return inputName;
            }
        }
    }

    public struct DataStateTypes
    {
        public static readonly string TRANSLATIONS = "Translations";
    }

    public struct Logging
    {
        public static readonly string DEFAULT_LOG_FORMAT = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} ${uppercase:${level}} ${logger}: ${message} ${exception:format=tostring}";
        public static readonly string MAIN_LOG_NAME = "MainLog";
        public static readonly string MAIN_LOG_RULE_NAME = "MainLogRule";
    }

    public struct PreferenceKeys
    {
        public static readonly string CULTURE = "Culture";
        public static readonly string CULTURE_DEFAULT = "en";
        public static readonly string LOG_INTERNAL_LOG_ON = "InternalLogOn";
        public static readonly string LOG_LEVEL = "LogLevel";
        public static readonly string SIZE_HEIGHT = "SizeHeight";
        public static readonly string SIZE_WIDTH = "SizeWidth";
    }

    public struct SQLiteConstants
    {
        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;
    }
}
