using Microsoft.EntityFrameworkCore;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Personnel> Personnel => Set<Personnel>();
    public DbSet<Activite>  Activites => Set<Activite>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Read connection string from environment variable or use a default.
        // Set GESTABS_CONN in your environment, e.g.:
        //   Host=localhost;Database=gestabs;Username=postgres;Password=yourpassword
        var conn = System.Environment.GetEnvironmentVariable("GESTABS_CONN")
                   ?? "Host=localhost;Database=gestabs;Username=postgres;Password=1234";

        options.UseNpgsql(conn);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Personnel: PPR is the natural key (no auto-increment)
        modelBuilder.Entity<Personnel>()
            .HasKey(p => p.Ppr);

        modelBuilder.Entity<Personnel>()
            .Property(p => p.Ppr)
            .ValueGeneratedNever();

        // Activite: auto-increment surrogate key
        modelBuilder.Entity<Activite>()
            .HasKey(a => a.Id);

        // One Personnel → many Activites
        modelBuilder.Entity<Activite>()
            .HasOne(a => a.Personnel)
            .WithMany()
            .HasForeignKey(a => a.Ppr)
            .OnDelete(DeleteBehavior.Cascade);
    }
}