using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MyAvaloniaApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MyAvaloniaApp.Services;

public class AbsenceCardService
{
    private static readonly Dictionary<string, string> TypeColors = new()
    {
        ["غياب بدون مبرر"] = "FDE68A",
        ["رخصة مرض"]       = "BBF7D0",
        ["رخصة إدارية"]    = "BFDBFE",
        ["رخصة الحج"]      = "DDD6FE",
        ["رخصة بدون أجر"]  = "FED7AA",
        ["رخصة أمومة"]     = "FBCFE8",
        ["إضراب"]          = "FECACA",
    };

    private static readonly (string Name, int MonthNumber)[] SchoolMonths =
    {
        ("شتنبر",  9),  ("أكتوبر", 10), ("نونبر",  11), ("دجنبر",  12),
        ("يناير",  1),  ("فبراير", 2),  ("مارس",   3),  ("أبريل",  4),
        ("ماي",    5),  ("يونيو",  6),
    };

    private static string? FindLogoPath()
    {
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
        var candidates = new[]
        {
            Path.Combine(exeDir, "logo_men.jpg"),
            Path.Combine(exeDir, "Assets", "logo_men.jpg"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo_men.jpg"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo_men.jpg"),
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    private static string Fmt(decimal v) =>
        v == 0 ? "0" : v % 1 == 0 ? ((int)v).ToString() : v.ToString("G");

    // ── Compute totals helper ─────────────────────────────────────────────────
    private static (decimal totalHours, int totalDays, decimal greveHours, int greveDays)
        ComputeTotals(List<Absence> absences)
    {
        var absenceOnly = absences.Where(a => a.AbsenceType != "إضراب").ToList();
        var greveOnly   = absences.Where(a => a.AbsenceType == "إضراب").ToList();

        var totalHours = absenceOnly
            .GroupBy(a => a.AbsenceDate.Date)
            .Where(g => g.Sum(a => a.Hours) < 4)
            .Sum(g => g.Sum(a => a.Hours));

        var totalDays = absenceOnly
            .GroupBy(a => a.AbsenceDate.Date)
            .Count(g => g.Sum(a => a.Hours) >= 4);

        var greveHours = greveOnly.Sum(a => a.Hours);
        var greveDays  = greveOnly
            .GroupBy(a => a.AbsenceDate.Date)
            .Count(g => g.Sum(a => a.Hours) >= 4);

        return (totalHours, totalDays, greveHours, greveDays);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PUBLIC: Single employee → Landscape A4, fills full page
    // ═════════════════════════════════════════════════════════════════════════
    public void Generate(
        Personnel employee,
        List<Absence> absences,
        string institutionName,
        string academicYear,
        string outputPath)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var lookup    = absences.ToDictionary(a => (a.AbsenceDate.Month, a.AbsenceDate.Day), a => a);
        var totals    = ComputeTotals(absences);
        var startYear = ParseStartYear(academicYear);
        var logoPath  = FindLogoPath();
        var today     = DateTime.Now.ToString("dd/MM/yyyy");

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginHorizontal(8);
                page.MarginVertical(6);
                page.DefaultTextStyle(ts => ts.FontFamily("Arial").FontSize(10).FontColor("1E293B"));

                page.Content().Column(col =>
                    RenderCard(col, employee, lookup, totals, startYear,
                               institutionName, academicYear, logoPath, today,
                               compact: false, extendGrid: true));
            });
        }).GeneratePdf(outputPath);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PUBLIC: All employees → Portrait A4, 2 per page filling full page
    // ═════════════════════════════════════════════════════════════════════════
    public void GenerateAll(
        List<(Personnel Employee, List<Absence> Absences)> data,
        string institutionName,
        string academicYear,
        string outputPath)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var startYear = ParseStartYear(academicYear);
        var logoPath  = FindLogoPath();
        var today     = DateTime.Now.ToString("dd/MM/yyyy");

        var pairs = new List<(Personnel, List<Absence>, Personnel?, List<Absence>?)>();
        for (int i = 0; i < data.Count; i += 2)
        {
            var first  = data[i];
            var second = i + 1 < data.Count ? data[i + 1] : ((Personnel?)null, (List<Absence>?)null);
            pairs.Add((first.Employee, first.Absences, second.Item1, second.Item2));
        }

        Document.Create(container =>
        {
            foreach (var (emp1, abs1, emp2, abs2) in pairs)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(7);
                    page.MarginVertical(6);
                    page.DefaultTextStyle(ts => ts.FontFamily("Arial").FontSize(9).FontColor("1E293B"));

                    // Use a Grid-like structure: two equal rows filling the full page
                    page.Content().Column(col =>
                    {
                        var lookup1 = abs1.ToDictionary(a => (a.AbsenceDate.Month, a.AbsenceDate.Day), a => a);
                        var totals1 = ComputeTotals(abs1);

                        if (emp2 is not null && abs2 is not null)
                        {
                            // Two cards — each gets exactly half the page height
                            col.Item().Height(405).Column(half =>
                                RenderCard(half, emp1, lookup1, totals1, startYear,
                                           institutionName, academicYear, logoPath, today,
                                           compact: true, extendGrid: true));

                            col.Item().Height(4);
                            col.Item().LineHorizontal(1).LineColor("B0BEC5");
                            col.Item().Height(4);

                            var lookup2 = abs2.ToDictionary(a => (a.AbsenceDate.Month, a.AbsenceDate.Day), a => a);
                            var totals2 = ComputeTotals(abs2);

                            col.Item().Height(405).Column(half =>
                                RenderCard(half, emp2, lookup2, totals2, startYear,
                                           institutionName, academicYear, logoPath, today,
                                           compact: true, extendGrid: true));
                        }
                        else
                        {
                            // Only one card on this page — fill full page
                            col.Item().Extend().Column(half =>
                                RenderCard(half, emp1, lookup1, totals1, startYear,
                                           institutionName, academicYear, logoPath, today,
                                           compact: true, extendGrid: true));
                        }
                    });
                });
            }
        }).GeneratePdf(outputPath);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PRIVATE: Renders one card into a column container
    // compact = true  → smaller fonts/spacing for 2-per-page portrait
    // compact = false → full size for single landscape
    // ═════════════════════════════════════════════════════════════════════════
    private static void RenderCard(
        ColumnDescriptor col,
        Personnel employee,
        Dictionary<(int, int), Absence> lookup,
        (decimal totalHours, int totalDays, decimal greveHours, int greveDays) totals,
        int startYear,
        string institutionName,
        string academicYear,
        string? logoPath,
        string today,
        bool compact,
        bool extendGrid = false) // kept for compatibility
    {
        var latName = $"{employee.NomLatin} {employee.PrenomLatin}".Trim();
        var arName  = $"{employee.NomArabe} {employee.PrenomArabe}".Trim();
        var name    = string.IsNullOrWhiteSpace(latName) ? arName : latName;

        const string headerBg   = "1A3A6B";
        const string headerText = "FFFFFF";
        const string accentBg   = "EBF0FA";
        const string borderClr  = "B0BEC5";
        const string cellBorder = "CFD8DC";

        float titleFs   = compact ? 16f : 22f;
        float yearFs    = compact ? 12f : 16f;
        float labelFs   = compact ? 11f : 15f;   // employee info — bigger
        float cellFs    = compact ? 7f  : 9f;
        float monthFs   = compact ? 7f  : 10f;
        float summaryFs = compact ? 18f : 28f;
        float hdrHeight = compact ? 30f : 80f;  // header — taller for bigger logo
        float hdrLogoW  = compact ? 150f : 300f; // logo — much wider
        float hdrInfoW  = compact ? 150f : 190f;
        float spacing   = compact ? 3f  : 6f;

        // ── HEADER ──────────────────────────────────────────────────────────
        col.Item()
            .Border(2).BorderColor(headerBg)
            .Background(headerBg)
            .Row(row =>
            {
                // School info — LEFT
                row.ConstantItem(hdrInfoW).Padding(compact ? 6f : 10f).AlignMiddle().Column(c =>
                {
                    c.Item().AlignLeft()
                        .Text(institutionName)
                        .Bold().FontSize(compact ? 11f : 14f).FontColor(headerText);
                    c.Item().Height(3);
                    c.Item().AlignLeft()
                        .Text($"الموسم الدراسي: {academicYear}")
                        .FontSize(compact ? 9f : 12f).FontColor("90CAF9");
                });

                // Title — CENTER
                row.RelativeItem().AlignCenter().AlignMiddle().Column(c =>
                {
                    c.Item().AlignCenter()
                        .Text("بطاقة الغياب السنوية")
                        .Bold().FontSize(titleFs).FontColor(headerText);
                    c.Item().Height(3);
                    c.Item().AlignCenter()
                        .Text(academicYear)
                        .Bold().FontSize(yearFs).FontColor("90CAF9");
                });

                // Logo — RIGHT (bigger)
                row.ConstantItem(hdrLogoW)
                    .Padding(compact ? 5f : 8f).AlignMiddle().Height(hdrHeight)
                    .Element(el =>
                    {
                        if (logoPath is not null)
                            el.Image(logoPath, ImageScaling.FitArea);
                        else
                            el.AlignCenter().AlignMiddle()
                              .Text("وزارة التربية الوطنية")
                              .Bold().FontSize(9).FontColor(headerText);
                    });
            });

        col.Item().Height(spacing);

        // ── EMPLOYEE INFO ────────────────────────────────────────────────────
        col.Item()
            .Border(1.5f).BorderColor(headerBg)
            .Background(accentBg)
            .Column(info =>
            {
                info.Item().Background(headerBg).Padding(3).AlignCenter()
                    .Text("بيانات الموظف(ة)")
                    .Bold().FontSize(labelFs).FontColor(headerText);

                info.Item().Padding(compact ? 5f : 8f).Row(row =>
                {
                    // LEFT: التخصص + الجنس (equal third)
                    row.RelativeItem().Padding(3).Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().AlignLeft().Text(employee.CdDiscip ?? "—").FontSize(labelFs - 1);
                            r.ConstantItem(4);
                            r.AutoItem().Text(": التخصص").Bold().FontSize(labelFs - 1);
                        });
                        c.Item().Height(3);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().AlignLeft()
                             .Text(employee.Genre == "M" ? "ذكر" : "أنثى").FontSize(labelFs - 1);
                            r.ConstantItem(4);
                            r.AutoItem().Text(": الجنس").Bold().FontSize(labelFs - 1);
                        });
                    });

                    row.ConstantItem(1).Background(borderClr);

                    // MIDDLE: رقم التأجير + الإطار (equal third)
                    row.RelativeItem().Padding(3).Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().AlignLeft().Text(employee.Ppr.ToString()).FontSize(labelFs);
                            r.ConstantItem(4);
                            r.AutoItem().Text(": رقم التأجير").Bold().FontSize(labelFs);
                        });
                        c.Item().Height(3);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().AlignLeft().Text(employee.CdCadre ?? "—").FontSize(labelFs - 1);
                            r.ConstantItem(4);
                            r.AutoItem().Text(": الإطار").Bold().FontSize(labelFs - 1);
                        });
                    });

                    row.ConstantItem(1).Background(borderClr);

                    // RIGHT: الاسم (equal third)
                    row.RelativeItem().Padding(3).Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().AlignLeft().Text(name).FontSize(labelFs).Bold();
                            r.ConstantItem(4);
                            r.AutoItem().Text(": الاسم الكامل").Bold().FontSize(labelFs);
                        });
                        c.Item().Height(3);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().AlignLeft().Text(arName).FontSize(labelFs - 1);
                            r.ConstantItem(4);
                            r.AutoItem().Text(": الاسم بالعربية").Bold().FontSize(labelFs - 1);
                        });
                    });
                });
            });

        col.Item().Height(spacing);

        // ── LEGEND ───────────────────────────────────────────────────────────
        col.Item().Row(row =>
        {
            foreach (var (type, color) in TypeColors.Reverse())
            {
                row.AutoItem().Border(1).BorderColor(borderClr).Background(color)
                    .Padding(compact ? 2f : 3f).Text(type).FontSize(cellFs - 1);
                row.ConstantItem(2);
            }
            row.ConstantItem(4);
            row.AutoItem().AlignMiddle()
                .Text(":دلالة الألوان").Bold().FontSize(cellFs);
        });

        col.Item().Height(spacing - 1);

        // ── CALENDAR GRID ────────────────────────────────────────────────────
        col.Item().Border(1.5f).BorderColor(headerBg).Table(table =>
        {
            table.ExtendLastCellsToTableBottom();

            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(compact ? 16f : 20f);
                cd.ConstantColumn(compact ? 16f : 20f);
                for (int d = 1; d <= 31; d++)
                    cd.RelativeColumn(1);
                cd.ConstantColumn(compact ? 22f : 28f);
            });

            table.Header(h =>
            {
                void HCell(string text) =>
                    h.Cell().Border(1).BorderColor(headerBg)
                        .Background(headerBg).AlignCenter().AlignMiddle()
                        .Text(text).Bold().FontSize(cellFs).FontColor(headerText);

                void DCell(int d) =>
                    h.Cell().Border(1).BorderColor(borderClr)
                        .Background("DBEAFE").AlignCenter().AlignMiddle()
                        .Text(d.ToString()).Bold().FontSize(cellFs).FontColor("1A3A6B");

                HCell("ساعات");
                HCell("أيام");
                for (int d = 31; d >= 1; d--) DCell(d);
                HCell("الشهر");
            });

            foreach (var (monthName, monthNum) in SchoolMonths)
            {
                var year        = monthNum >= 9 ? startYear : startYear + 1;
                var daysInMonth = DateTime.DaysInMonth(year, monthNum);

                decimal mHours = 0; int mDays = 0;
                for (int d = 1; d <= daysInMonth; d++)
                {
                    if (lookup.TryGetValue((monthNum, d), out var a) && a.AbsenceType != "إضراب")
                    {
                        if (a.Hours >= 4) mDays++;
                        else mHours += a.Hours;
                    }
                }

                table.Cell().Border(1).BorderColor(cellBorder)
                    .Background(mHours > 0 ? "FEE2E2" : accentBg).AlignCenter().AlignMiddle()
                    .MinHeight(compact ? 18f : 22f)
                    .Text(mHours > 0 ? Fmt(mHours) : "").Bold().FontSize(cellFs).FontColor("DC2626");

                table.Cell().Border(1).BorderColor(cellBorder)
                    .Background(mDays > 0 ? "FEE2E2" : accentBg).AlignCenter().AlignMiddle()
                    .MinHeight(compact ? 18f : 22f)
                    .Text(mDays > 0 ? mDays.ToString() : "").Bold().FontSize(cellFs).FontColor("DC2626");

                for (int d = 31; d >= 1; d--)
                {
                    if (d > daysInMonth)
                        table.Cell().Border(1).BorderColor(cellBorder).Background("ECEFF1").Text("");
                    else if (lookup.TryGetValue((monthNum, d), out var ab))
                    {
                        var bg = TypeColors.TryGetValue(ab.AbsenceType, out var c) ? c : "FDE68A";
                        table.Cell().Border(1).BorderColor(cellBorder).Background(bg)
                            .AlignCenter().AlignMiddle()
                            .Text(Fmt(ab.Hours)).FontSize(cellFs).Bold().FontColor("1E293B");
                    }
                    else
                        table.Cell().Border(1).BorderColor(cellBorder).Background("FFFFFF").Text("");
                }

                table.Cell().Border(1).BorderColor(cellBorder).Background(headerBg)
                    .AlignCenter().AlignMiddle().MinHeight(compact ? 18f : 22f)
                    .Text(monthName).Bold().FontSize(monthFs).FontColor(headerText);
            }
        });

        col.Item().Height(spacing - 1);

        // ── FOOTER SUMMARY ────────────────────────────────────────────────────
        col.Item().Border(1.5f).BorderColor(headerBg).Background(accentBg).Column(footer =>
        {
            footer.Item().Background(headerBg).Padding(3).AlignCenter()
                .Text($"سجل لهذا الموظف(ة) إلى حدود يوم: {today}")
                .Bold().FontSize(labelFs).FontColor(headerText);

            footer.Item().Padding(compact ? 4f : 8f).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(borderClr)
                    .Background("FFFFFF").Padding(compact ? 4f : 6f).Column(c =>
                {
                    c.Item().AlignCenter()
                        .Text("من الغياب عن العمل ما مجموعه:").Bold().FontSize(labelFs);
                    c.Item().Height(3);
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().AlignCenter().Column(dc =>
                        {
                            dc.Item().AlignCenter().Text(totals.totalDays.ToString())
                                .Bold().FontSize(summaryFs).FontColor("DC2626");
                            dc.Item().AlignCenter().Text("يوماً")
                                .FontSize(labelFs - 1).FontColor("475569");
                        });
                        r.ConstantItem(1).Background(borderClr);
                        r.RelativeItem().AlignCenter().Column(dc =>
                        {
                            dc.Item().AlignCenter().Text(Fmt(totals.totalHours))
                                .Bold().FontSize(summaryFs).FontColor("DC2626");
                            dc.Item().AlignCenter().Text("ساعة")
                                .FontSize(labelFs - 1).FontColor("475569");
                        });
                    });
                });

                row.ConstantItem(compact ? 4f : 8f);

                row.RelativeItem().Border(1).BorderColor(borderClr)
                    .Background("FFFFFF").Padding(compact ? 4f : 6f).Column(c =>
                {
                    c.Item().AlignCenter()
                        .Text("ومن الإضراب عن العمل ما مجموعه:").Bold().FontSize(labelFs);
                    c.Item().Height(3);
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().AlignCenter().Column(dc =>
                        {
                            dc.Item().AlignCenter().Text(totals.greveDays.ToString())
                                .Bold().FontSize(summaryFs).FontColor("7C3AED");
                            dc.Item().AlignCenter().Text("يوماً")
                                .FontSize(labelFs - 1).FontColor("475569");
                        });
                        r.ConstantItem(1).Background(borderClr);
                        r.RelativeItem().AlignCenter().Column(dc =>
                        {
                            dc.Item().AlignCenter().Text(Fmt(totals.greveHours))
                                .Bold().FontSize(summaryFs).FontColor("7C3AED");
                            dc.Item().AlignCenter().Text("ساعة")
                                .FontSize(labelFs - 1).FontColor("475569");
                        });
                    });
                });
            });
        });
    }

    private static int ParseStartYear(string academicYear)
    {
        var startYear = 2025;
        if (academicYear.Contains('/'))
            int.TryParse(academicYear.Split('/')[0].Trim(), out startYear);
        return startYear;
    }
}