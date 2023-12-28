using PWP.Maui.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PWP.Maui.Infrastructure.Data;

public interface IPWPMauiDataContext
{
    DbSet<Culture> Cultures { get; }
    DatabaseFacade Database { get; }
    DbSet<TranslationArea> TranslationAreas { get; }
    DbSet<Translation> Translations { get; }

    void Dispose();
    void Initialize();
}