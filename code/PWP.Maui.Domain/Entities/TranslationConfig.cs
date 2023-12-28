namespace PWP.Maui.Domain.Entities;

public class TCCulture
{
    public string? TwoLetterCultureCode { get; set; }
    public string? FullCultureCode { get; set; }
    public string? TwoLetterFlagCode { get; set; }
}

public class TCTranslation
{
    public string? Culture { get; set; }
    public string? Key { get; set; }
    public string? Value { get; set; }
}

public class TCTranslationArea
{
    public string? Name { get; set; }
    public List<TCTranslation>? TCTranslations { get; set; }
}

public class TranslationConfig
{
    public List<TCCulture>? TCCultures { get; set; }
    public List<TCTranslationArea>? TCTranslationAreas { get; set; }
}