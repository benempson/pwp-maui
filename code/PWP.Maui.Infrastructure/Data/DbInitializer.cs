using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PWP.Maui.Domain;
using PWP.Maui.Domain.Entities;
using PWP.Maui.Utils;
using PWP.Maui.Utils.Extensions;
using System.Reflection;
using System.Text.Json;

namespace PWP.Maui.Infrastructure.Data;

public class DbInitializer
{
    private readonly DataContext _dataContext;
    private readonly ILogger<DbInitializer> _logger;
    private readonly ModelBuilder? _modelBuilder;

    public DbInitializer(ILoggerFactory loggerFactory, DataContext dataContext)
    {
        _dataContext = dataContext;
        _logger = loggerFactory.CreateLogger<DbInitializer>();
        _logger.LogFunctionStart();
    }

    public DbInitializer(ModelBuilder modelBuilder, ILoggerFactory loggerFactory, DataContext dataContext)
    {
        _dataContext = dataContext;
        _logger = loggerFactory.CreateLogger<DbInitializer>();
        _logger.LogFunctionStart();
        _modelBuilder = modelBuilder;
    }

    public void OnModelCreating()
    {

    }

    public void Initialize()
    {
        _logger.LogFunctionStart();

        Assembly assem = GetType().Assembly;
        string configFileName = "TranslationData.json";
        string configFullName = assem.GetManifestResourceNames().Single(r => r.EndsWith(configFileName));
        string? configText = null;

        using (Stream? stream = assem.GetManifestResourceStream(configFullName))
        {
            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    configText = reader.ReadToEnd();
                }
            }
        }

        string hashCode = configText!.ToSHA1Hash();
        DataState? ds = _dataContext.DataStates.SingleOrDefault(s => s.Type!.Equals(AppConstants.DataStateTypes.TRANSLATIONS));
        if (ds == null)
        {
            ds = new DataState { Type = AppConstants.DataStateTypes.TRANSLATIONS, StateHash = "" };
            _dataContext.DataStates.Add(ds);
        }

        //no need to apply this data if we've already done it
        if (ds.StateHash == hashCode)
            return;

        if (configText == null)
            return;

        TranslationConfig? tc = JsonSerializer.Deserialize<TranslationConfig>(configText);
        Guard.ThrowIfNull(tc);
        Guard.ThrowIfNull(tc.TCCultures);

        _dataContext.Database.ExecuteSqlRaw($"DELETE FROM {nameof(_dataContext.Translations)}");
        _dataContext.Database.ExecuteSqlRaw($"DELETE FROM {nameof(_dataContext.TranslationAreas)}");
        _dataContext.Database.ExecuteSqlRaw($"DELETE FROM {nameof(_dataContext.Cultures)}");

        foreach (TCCulture tcc in tc.TCCultures)
        {
            Culture c = new Culture { TwoLetterCultureCode = tcc.TwoLetterCultureCode, FullCultureCode = tcc.FullCultureCode, TwoLetterFlagCode = tcc.TwoLetterFlagCode };
            _dataContext.Cultures.Add(c);
        }
        _dataContext.SaveChanges();

        Guard.ThrowIfNull(tc.TCTranslationAreas);
        foreach (TCTranslationArea tcta in tc.TCTranslationAreas)
        {
            TranslationArea ta = new TranslationArea { Name = tcta.Name };
            _dataContext.TranslationAreas.Add(ta);
        }
        _dataContext.SaveChanges();

        foreach (TCTranslationArea tcta in tc.TCTranslationAreas)
        {
            Guard.ThrowIfNull(tcta.TCTranslations);
            foreach (TCTranslation tct in tcta.TCTranslations)
            {
                Translation t = new Translation
                {
                    CultureId = _dataContext.Cultures.Single(c => c.TwoLetterCultureCode == tct.Culture).Id,
                    AreaId = _dataContext.TranslationAreas.Single(ta => ta.Name == tcta.Name).Id,
                    Key = tct.Key,
                    Value = tct.Value
                };
                _dataContext.Translations.Add(t);
            }
        }
        _dataContext.SaveChanges();

        if (ds.StateHash != hashCode)
        {
            ds.StateHash = hashCode;
            _dataContext.SaveChanges();
        }

        _logger.LogFunctionEnd();
    }
}