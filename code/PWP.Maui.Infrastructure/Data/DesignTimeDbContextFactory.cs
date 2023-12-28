using PWP.Maui.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PWP.Maui.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseSqlite($"Data Source={new RuntimeValues(Environment.SpecialFolder.LocalApplicationData.ToString()).DbFilename}");
        return new DataContext(optionsBuilder.Options);
    }
}