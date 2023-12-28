using PWP.Maui.Domain;
using PWP.Maui.Domain.Entities;
using PWP.Maui.Infrastructure.Data;
using PWP.Maui.Infrastructure.Services.Interfaces;
using PWP.Maui.Utils.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace PWP.Maui.Infrastructure.Services.Implementations;

public class TranslationService : ITranslationService
{
    //https://mudblazor.com/features/localization#translation-keys

    private IDataContext? _dataContext = null;
    private List<TranslationArea> _areas = new List<TranslationArea>();
    private bool _cultureIsSupported = true;
    private ILogger<TranslationService> _logger;
    private IPlatformPreferences _platformPreferences;
    private RuntimeValues _runtimeValues;
    private List<Translation> _translations = new List<Translation>();
    private List<string> _supportedCultures = new List<string>();

    public TranslationService(IDataContext dataContext, ILogger<TranslationService> logger, IPlatformPreferences platformPreferences, RuntimeValues runtimeValues)
    {
        _dataContext = dataContext;
        _logger = logger;
        _platformPreferences = platformPreferences;
        _runtimeValues = runtimeValues;
        Initialize();
        LoadTranslations();
    }

    public LocalizedString this[string key]
    {
        get
        {
            if (_runtimeValues.DbInitialized)
            {
                string areaKey = key.Substring(0, key.IndexOf('.'));
                TranslationArea? area = _areas.SingleOrDefault(a => !string.IsNullOrEmpty(a.Name) && a.Name.Equals(areaKey));
                if (area == null)
                    return new LocalizedString(key, $"[{key}] [T:ANF]", true);

                string translationKey = key.Replace($"{areaKey}.", "");
                Translation? t = _translations.SingleOrDefault(t1 => t1.AreaId == area.Id && !string.IsNullOrEmpty(t1.Key) && t1.Key.Equals(translationKey));

                if (t != null)
                {
                    if (_cultureIsSupported)
                        return new LocalizedString(key, t.Value ?? "[T:VIN]");
                    else
                        return new LocalizedString(key, $"{t.Value} [T:CNF]", true);
                }
                else
                    return new LocalizedString(key, $"[{key}] [T:TNF]", true);
            }
            else
                return new LocalizedString(key, $"[{key}] [T:DNI]", true);
        }
    }

    /// <summary>
    /// Remove all cached translation data
    /// </summary>
    public void ClearValues()
    {
        _supportedCultures.Clear();
        _areas.Clear();
        _translations.Clear();
    }

    /// <summary>
    /// Load Cultures and Areas
    /// </summary>
    public void Initialize()
    {
        _logger.LogFunctionStart();

        if (_runtimeValues.DbInitialized && _supportedCultures.Count == 0 && _dataContext != null)
        {
            _logger.LogWithFunctionName("Loading data from db", LogLevel.Debug);
            ClearValues();

            foreach (Culture l in _dataContext.Cultures)
                _supportedCultures.Add(l.TwoLetterCultureCode!);

            foreach (TranslationArea a in _dataContext.TranslationAreas)
                _areas.Add(a);
        }
    }

    /// <summary>
    /// Load translations according to the current culture in use
    /// </summary>
    public void LoadTranslations()
    {
        if (_runtimeValues.DbInitialized && _dataContext != null)
        {
            if (_supportedCultures.Count == 0)
                Initialize();

            _logger.LogWithFunctionName("Loading data from db", LogLevel.Debug);
            _translations.Clear();

            Culture? culture = null;
            string currentCulture = _platformPreferences.Get(AppConstants.PreferenceKeys.CULTURE, AppConstants.PreferenceKeys.CULTURE_DEFAULT);

            if (!_supportedCultures.Contains(currentCulture))
            {
                CultureInfo ci = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => c.Name.StartsWith($"-{currentCulture}")).First();
                currentCulture = ci.Parent.Name;
                if (!_supportedCultures.Contains(currentCulture))
                {
                    currentCulture = "en";
                    _cultureIsSupported = false;
                }
            }

            culture = _dataContext.Cultures.Single(l => l.TwoLetterCultureCode == currentCulture);
            if (culture != null)
            {
                foreach (Translation t in _dataContext.Translations.Where(t1 => t1.CultureId == culture.Id))
                    _translations.Add(t);
            }
        }
    }
}
