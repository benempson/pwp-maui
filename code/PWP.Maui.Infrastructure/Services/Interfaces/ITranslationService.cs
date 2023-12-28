using Microsoft.Extensions.Localization;

namespace PWP.Maui.Infrastructure.Services.Interfaces;

public interface ITranslationService
{
    LocalizedString this[string key] { get; }
    void LoadTranslations();
}
