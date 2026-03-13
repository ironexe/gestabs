using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyAvaloniaApp.Data;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Services;

public class AbsenceService
{
    public async Task<List<Personnel>> GetAllPersonnelAsync()
    {
        await using var db = new AppDbContext();
        return await db.Personnel
            .OrderBy(p => p.NomLatin)
            .ToListAsync();
    }

    public async Task<List<Absence>> GetAbsencesForPersonnelAsync(int ppr)
    {
        await using var db = new AppDbContext();
        return await db.Absences
            .Where(a => a.Ppr == ppr)
            .OrderByDescending(a => a.AbsenceDate)
            .ToListAsync();
    }

    public async Task AddAbsenceAsync(Absence absence)
    {
        await using var db = new AppDbContext();
        await db.Absences.AddAsync(absence);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAbsenceAsync(Absence updated)
    {
        await using var db = new AppDbContext();
        var existing = await db.Absences.FindAsync(updated.Id);
        if (existing is null) return;

        existing.AbsenceDate  = updated.AbsenceDate;
        existing.Hours        = updated.Hours;
        existing.AbsenceType  = updated.AbsenceType;
        existing.Notes        = updated.Notes;

        db.Absences.Update(existing);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAbsenceAsync(int id)
    {
        await using var db = new AppDbContext();
        var existing = await db.Absences.FindAsync(id);
        if (existing is null) return;

        db.Absences.Remove(existing);
        await db.SaveChangesAsync();
    }
}