using Microsoft.EntityFrameworkCore;
using ITCentral.Models;
using ProviderName = LinqToDB.ProviderName;
using ITCentral.Common;

namespace ITCentral.Data
{
    public class EntityCreate : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _ = AppCommon.DbType switch
            {
                ProviderName.SQLite => optionsBuilder.UseSqlite(AppCommon.ConnectionString),
                ProviderName.SqlServer => optionsBuilder.UseSqlServer(AppCommon.ConnectionString),
                ProviderName.PostgreSQL => optionsBuilder.UseNpgsql(AppCommon.ConnectionString),
                _ => throw new InvalidOperationException("Unsupported database type")
            };
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Extraction>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.FilterColumn);
                entity.Property(e => e.FilterTime);
                entity.Property(e => e.ScheduleId).IsRequired();
                entity.Property(e => e.OriginId).IsRequired();
                entity.Property(e => e.DestinationId).IsRequired();
                entity.Property(e => e.IndexName).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.FileStructure);

                entity.HasOne(e => e.Schedule)
                      .WithMany()
                      .HasForeignKey(e => e.ScheduleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Origin)
                      .WithMany()
                      .HasForeignKey(e => e.OriginId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Destination)
                      .WithMany()
                      .HasForeignKey(e => e.DestinationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Origin>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Destination>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Record>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.HostName).IsRequired();
                entity.Property(e => e.TimeStamp).IsRequired();
                entity.Property(e => e.EventType).IsRequired();
                entity.Property(e => e.CallerMethod).IsRequired();
                entity.Property(e => e.Event).IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Password);
            });
        }
    }
}