using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyAvaloniaApp.Models;

[Table("absences")]
public class Absence
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("ppr")]
    public int Ppr { get; set; }

    // Stored as date only (no time), UTC kind required by Npgsql
    private DateTime _absenceDate;
    [Column("absence_date")]
    public DateTime AbsenceDate
    {
        get => _absenceDate;
        set => _absenceDate = DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
    }

    [Column("hours")]
    public decimal Hours { get; set; }

    [Column("absence_type")]
    public string AbsenceType { get; set; } = "غياب بدون مبرر";

    [Column("notes")]
    public string? Notes { get; set; }

    private DateTime _createdAt = DateTime.UtcNow;
    [Column("created_at")]
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => _createdAt = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    [ForeignKey(nameof(Ppr))]
    public Personnel? Personnel { get; set; }
}