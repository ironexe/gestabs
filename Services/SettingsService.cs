using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyAvaloniaApp.Data;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Services;

public class SettingsService
{
    /// <summary>Returns true if no settings row exists yet (first run).</summary>
    public async Task<bool> IsFirstRunAsync()
    {
        await using var db = new AppDbContext();
        await db.Database.EnsureCreatedAsync();
        return !await db.AppSettings.AnyAsync();
    }

    public async Task<AppSettings> LoadAsync()
    {
        await using var db = new AppDbContext();
        await db.Database.EnsureCreatedAsync();

        var settings = await db.AppSettings.FirstOrDefaultAsync();
        if (settings is null)
        {
            settings = new AppSettings();
            await db.AppSettings.AddAsync(settings);
            await db.SaveChangesAsync();
        }
        return settings;
    }

    public async Task SaveAsync(AppSettings settings)
    {
        await using var db = new AppDbContext();
        var existing = await db.AppSettings.FirstOrDefaultAsync();
        if (existing is null)
        {
            await db.AppSettings.AddAsync(settings);
        }
        else
        {
            existing.InstitutionName = settings.InstitutionName;
            existing.AcademyName     = settings.AcademyName;
            existing.RegionName      = settings.RegionName;
            existing.AcademicYear    = settings.AcademicYear;
            existing.DirectorName    = settings.DirectorName;
            db.AppSettings.Update(existing);
        }
        await db.SaveChangesAsync();
    }
}