using ITCentral.Common;
using Microsoft.EntityFrameworkCore;

namespace ITCentral.Service;

public abstract class ServiceBase<T> : DbContext where T : class
{
    public ServiceBase()
    {
        Database.EnsureCreated();
        // Database.Migrate();
    }
    protected DbSet<T> Repository{get; set;} = null!;
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite(AppCommon.ConnectionString);

    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     base.OnModelCreating(modelBuilder);
    // }
}