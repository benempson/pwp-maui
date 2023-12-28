using PWP.Maui.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PWP.Maui.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PWPMauiDataContext>
{
    public PWPMauiDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PWPMauiDataContext>();
        optionsBuilder.UseSqlite($"Data Source={new RuntimeValues(Environment.SpecialFolder.LocalApplicationData.ToString()).DbFilename}");
        return new PWPMauiDataContext(optionsBuilder.Options);
    }
}