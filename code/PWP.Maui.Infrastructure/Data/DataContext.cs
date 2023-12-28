using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PWP.Maui.Domain.Entities;
using PWP.Maui.Utils.Extensions;

namespace PWP.Maui.Infrastructure.Data;

public class DataContext : DbContext, IDataContext
{
    private readonly ILogger<DataContext>? _logger;
    private readonly ILoggerFactory? _loggerFactory;

    public DbSet<Culture> Cultures { get { return Set<Culture>(); } }
    public override DatabaseFacade Database { get { return base.Database; } }
    public DbSet<DataState> DataStates { get {  return Set<DataState>(); } }
    public DbSet<TranslationArea> TranslationAreas { get { return Set<TranslationArea>(); } }
    public DbSet<Translation> Translations { get { return Set<Translation>(); } }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        _logger = new Logger<DataContext>(new LoggerFactory());
    }

    public DataContext(IServiceProvider serviceProvider, DbContextOptions<DataContext> options) : base(options)
    {
        _loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = _loggerFactory.CreateLogger<DataContext>();
        _logger.LogFunctionStart();
    }

    public override void Dispose()
    {
        _logger!.LogFunctionStart();
        base.Dispose();
    }

    public void Initialize()
    {
        new DbInitializer(_loggerFactory!, this).Initialize();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _logger!.LogFunctionStartWithArgs().Values(optionsBuilder.IsConfigured);

        if (!optionsBuilder.IsConfigured)
        {
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _logger!.LogFunctionStart();
        base.OnModelCreating(modelBuilder);
        new DbInitializer(modelBuilder, _loggerFactory!, this).OnModelCreating();
    }
}