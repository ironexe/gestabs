using Microsoft.EntityFrameworkCore;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Personnel>   Personnel   => Set<Personnel>();
    public DbSet<Activite>    Activites   => Set<Activite>();
    public DbSet<Absence>     Absences    => Set<Absence>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var conn = System.Environment.GetEnvironmentVariable("GESTABS_CONN")
                   ?? "Host=localhost;Database=gestabs;Username=postgres;Password=1234";
        options.UseNpgsql(conn);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Personnel>()
            .HasKey(p => p.Ppr);
        modelBuilder.Entity<Personnel>()
            .Property(p => p.Ppr)
            .ValueGeneratedNever();

        modelBuilder.Entity<Activite>()
            .HasKey(a => a.Id);
        modelBuilder.Entity<Activite>()
            .HasOne(a => a.Personnel)
            .WithMany()
            .HasForeignKey(a => a.Ppr)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Absence>()
            .HasKey(a => a.Id);
        modelBuilder.Entity<Absence>()
            .HasOne(a => a.Personnel)
            .WithMany()
            .HasForeignKey(a => a.Ppr)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppSettings>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<AppSettings>()
            .Property(s => s.Id)
            .ValueGeneratedNever();
    }
}