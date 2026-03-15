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

    public void Generate(
        Personnel employee,
        List<Absence> absences,
        string institutionName,
        string academicYear,
        string outputPath)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var lookup = absences.ToDictionary(
            a => (a.AbsenceDate.Month, a.AbsenceDate.Day), a => a);

        // Totals — exclude إضراب from absence totals
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

        var startYear = 2025;
        if (academicYear.Contains('/'))
            int.TryParse(academicYear.Split('/')[0].Trim(), out startYear);

        var latName = $"{employee.NomLatin} {employee.PrenomLatin}".Trim();
        var arName  = $"{employee.NomArabe} {employee.PrenomArabe}".Trim();
        var name    = string.IsNullOrWhiteSpace(latName) ? arName : latName;

        var logoPath  = FindLogoPath();
        var today     = DateTime.Now.ToString("dd/MM/yyyy");

        // ── BORDER COLORS ──────────────────────────────────────────────────────
        const string headerBg   = "1A3A6B";   // dark navy
        const string headerText = "FFFFFF";
        const string accentBg   = "EBF0FA";
        const string borderClr  = "B0BEC5";
        const string cellBorder = "CFD8DC";

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(14);
                page.MarginVertical(12);
                page.DefaultTextStyle(ts => ts.FontFamily("Arial").FontSize(7).FontColor("1E293B"));

                page.Content().Column(col =>
                {
                    // ════════════════════════════════════════════════════════
                    // HEADER
                    // ════════════════════════════════════════════════════════
                    col.Item()
                        .Border(2).BorderColor(headerBg)
                        .Background(headerBg)
                        .Padding(0)
                        .Row(row =>
                        {
                            // School info — LEFT side
                            row.ConstantItem(140).Padding(8).AlignMiddle().Column(c =>
                            {
                                c.Item().AlignLeft()
                                    .Text(institutionName)
                                    .Bold().FontSize(8).FontColor(headerText);
                                c.Item().Height(3);
                                c.Item().AlignLeft()
                                    .Text($"الموسم الدراسي: {academicYear}")
                                    .FontSize(7).FontColor("90CAF9");
                            });

                            // Title — CENTER
                            row.RelativeItem().AlignCenter().AlignMiddle().Column(c =>
                            {
                                c.Item().AlignCenter()
                                    .Text("بطاقة الغياب السنوية")
                                    .Bold().FontSize(16).FontColor(headerText);
                                c.Item().Height(3);
                                c.Item().AlignCenter()
                                    .Text(academicYear)
                                    .Bold().FontSize(12).FontColor("90CAF9");
                            });

                            // Logo — RIGHT side
                            row.ConstantItem(110).Padding(6).AlignMiddle().Height(52)
                                .Element(el =>
                                {
                                    if (logoPath is not null)
                                        el.Image(logoPath, ImageScaling.FitArea);
                                    else
                                        el.AlignCenter().AlignMiddle()
                                          .Text("وزارة التربية الوطنية")
                                          .Bold().FontSize(8).FontColor(headerText);
                                });
                        });

                    col.Item().Height(6);

                    // ════════════════════════════════════════════════════════
                    // EMPLOYEE INFO
                    // ════════════════════════════════════════════════════════
                    col.Item()
                        .Border(1.5f).BorderColor(headerBg)
                        .Background(accentBg)
                        .Padding(0)
                        .Column(info =>
                        {
                            // Info header bar
                            info.Item()
                                .Background(headerBg)
                                .Padding(4)
                                .AlignCenter()
                                .Text("بيانات الموظف(ة)")
                                .Bold().FontSize(8).FontColor(headerText);

                            info.Item().Padding(8).Row(row =>
                            {
                                // Added FIRST → renders on LEFT: التخصص + الجنس
                                row.ConstantItem(130).Padding(4).Column(c =>
                                {
                                    c.Item().Row(r =>
                                    {
                                        r.RelativeItem().AlignLeft().Text(employee.CdDiscip ?? "—").FontSize(7);
                                        r.ConstantItem(6);
                                        r.AutoItem().Text(": التخصص").Bold().FontSize(7);
                                    });
                                    c.Item().Height(4);
                                    c.Item().Row(r =>
                                    {
                                        r.RelativeItem().AlignLeft()
                                         .Text(employee.Genre == "M" ? "ذكر" : "أنثى").FontSize(7);
                                        r.ConstantItem(6);
                                        r.AutoItem().Text(": الجنس").Bold().FontSize(7);
                                    });
                                });

                                row.ConstantItem(1).Background(borderClr);

                                // Added SECOND → renders in MIDDLE: رقم التأجير + الإطار
                                row.ConstantItem(160).Padding(4).Column(c =>
                                {
                                    c.Item().Row(r =>
                                    {
                                        r.RelativeItem().AlignLeft().Text(employee.Ppr.ToString()).FontSize(8);
                                        r.ConstantItem(6);
                                        r.AutoItem().Text(": رقم التأجير").Bold().FontSize(8);
                                    });
                                    c.Item().Height(4);
                                    c.Item().Row(r =>
                                    {
                                        r.RelativeItem().AlignLeft().Text(employee.CdCadre ?? "—").FontSize(7);
                                        r.ConstantItem(6);
                                        r.AutoItem().Text(": الإطار").Bold().FontSize(7);
                                    });
                                });

                                row.ConstantItem(1).Background(borderClr);

                                // Added LAST → renders on RIGHT: الاسم الكامل + الاسم بالعربية
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Row(r =>
                                    {
                                        r.RelativeItem().AlignLeft().Text(name).FontSize(8).Bold();
                                        r.ConstantItem(6);
                                        r.AutoItem().Text(": الاسم الكامل").Bold().FontSize(8);
                                    });
                                    c.Item().Height(4);
                                    c.Item().Row(r =>
                                    {
                                        r.RelativeItem().AlignLeft().Text(arName).FontSize(7);
                                        r.ConstantItem(6);
                                        r.AutoItem().Text(": الاسم بالعربية").Bold().FontSize(7);
                                    });
                                });
                            });
                        });

                    col.Item().Height(6);

                    // ════════════════════════════════════════════════════════
                    // CALENDAR GRID  (RTL: ساعات | أيام | 31→1 | الشهر)
                    // ════════════════════════════════════════════════════════
                    col.Item().Border(1.5f).BorderColor(headerBg).Table(table =>
                    {
                        table.ColumnsDefinition(cd =>
                        {
                            cd.ConstantColumn(20);        // ساعات  (right)
                            cd.ConstantColumn(20);        // أيام
                            for (int d = 1; d <= 31; d++)
                                cd.RelativeColumn(1);     // days 31→1
                            cd.ConstantColumn(28);        // الشهر  (left)
                        });

                        // Header row
                        table.Header(h =>
                        {
                            void HCell(string text) =>
                                h.Cell().Border(1).BorderColor(headerBg)
                                    .Background(headerBg).AlignCenter().AlignMiddle()
                                    .Text(text).Bold().FontSize(6).FontColor(headerText);

                            void DayCell(int d) =>
                                h.Cell().Border(1).BorderColor(borderClr)
                                    .Background("DBEAFE").AlignCenter().AlignMiddle()
                                    .Text(d.ToString()).Bold().FontSize(6).FontColor("1A3A6B");

                            HCell("ساعات");
                            HCell("أيام");
                            for (int d = 31; d >= 1; d--) DayCell(d);
                            HCell("الشهر");
                        });

                        // Data rows
                        foreach (var (monthName, monthNum) in SchoolMonths)
                        {
                            var year        = monthNum >= 9 ? startYear : startYear + 1;
                            var daysInMonth = DateTime.DaysInMonth(year, monthNum);

                            // Pre-calc month totals
                            decimal mHours = 0; int mDays = 0;
                            for (int d = 1; d <= daysInMonth; d++)
                            {
                                if (lookup.TryGetValue((monthNum, d), out var a)
                                    && a.AbsenceType != "إضراب")
                                {
                                    if (a.Hours >= 4) mDays++;
                                    else mHours += a.Hours;
                                }
                            }

                            // Totals cells (right side)
                            table.Cell()
                                .Border(1).BorderColor(cellBorder)
                                .Background(mHours > 0 ? "FEE2E2" : accentBg)
                                .AlignCenter().AlignMiddle()
                                .Text(mHours > 0 ? Fmt(mHours) : "")
                                .Bold().FontSize(6).FontColor("DC2626");

                            table.Cell()
                                .Border(1).BorderColor(cellBorder)
                                .Background(mDays > 0 ? "FEE2E2" : accentBg)
                                .AlignCenter().AlignMiddle()
                                .Text(mDays > 0 ? mDays.ToString() : "")
                                .Bold().FontSize(6).FontColor("DC2626");

                            // Day cells 31 → 1
                            for (int d = 31; d >= 1; d--)
                            {
                                if (d > daysInMonth)
                                {
                                    table.Cell()
                                        .Border(1).BorderColor(cellBorder)
                                        .Background("ECEFF1").Text("");
                                }
                                else if (lookup.TryGetValue((monthNum, d), out var ab))
                                {
                                    var bg = TypeColors.TryGetValue(ab.AbsenceType, out var c)
                                        ? c : "FDE68A";
                                    table.Cell()
                                        .Border(1).BorderColor(cellBorder)
                                        .Background(bg)
                                        .AlignCenter().AlignMiddle()
                                        .Text(Fmt(ab.Hours))
                                        .FontSize(6).Bold().FontColor("1E293B");
                                }
                                else
                                {
                                    table.Cell()
                                        .Border(1).BorderColor(cellBorder)
                                        .Background("FFFFFF").Text("");
                                }
                            }

                            // Month name cell (left side)
                            table.Cell()
                                .Border(1).BorderColor(cellBorder)
                                .Background(headerBg)
                                .AlignCenter().AlignMiddle()
                                .Text(monthName).Bold().FontSize(7).FontColor(headerText);
                        }
                    });

                    col.Item().Height(5);

                    // ════════════════════════════════════════════════════════
                    // LEGEND  (RTL)
                    // ════════════════════════════════════════════════════════
                    col.Item().Row(row =>
                    {
                        // Badges right to left
                        foreach (var (type, color) in TypeColors.Reverse())
                        {
                            row.AutoItem()
                                .Border(1).BorderColor(borderClr)
                                .Background(color)
                                .Padding(3)
                                .Text(type).FontSize(6);
                            row.ConstantItem(3);
                        }
                        row.ConstantItem(6);
                        row.AutoItem().AlignMiddle()
                            .Text(":دلالة الألوان").Bold().FontSize(7);
                    });

                    col.Item().Height(5);

                    // ════════════════════════════════════════════════════════
                    // FOOTER SUMMARY
                    // ════════════════════════════════════════════════════════
                    col.Item()
                        .Border(1.5f).BorderColor(headerBg)
                        .Background(accentBg)
                        .Padding(0)
                        .Column(footer =>
                        {
                            // Footer header bar
                            footer.Item()
                                .Background(headerBg).Padding(4)
                                .AlignCenter()
                                .Text($"سجل لهذا الموظف(ة) إلى حدود يوم: {today}")
                                .Bold().FontSize(8).FontColor(headerText);

                            footer.Item().Padding(10).Row(row =>
                            {
                                // Absence summary
                                row.RelativeItem()
                                    .Border(1).BorderColor(borderClr)
                                    .Background("FFFFFF").Padding(8)
                                    .Column(c =>
                                    {
                                        c.Item().AlignCenter()
                                            .Text("من الغياب عن العمل ما مجموعه:")
                                            .Bold().FontSize(8);
                                        c.Item().Height(6);
                                        c.Item().Row(r =>
                                        {
                                            r.RelativeItem().AlignCenter().Column(dc =>
                                            {
                                                dc.Item().AlignCenter()
                                                    .Text(totalDays.ToString())
                                                    .Bold().FontSize(22).FontColor("DC2626");
                                                dc.Item().AlignCenter()
                                                    .Text("يوماً").FontSize(8).FontColor("475569");
                                            });
                                            r.ConstantItem(1).Background(borderClr);
                                            r.RelativeItem().AlignCenter().Column(dc =>
                                            {
                                                dc.Item().AlignCenter()
                                                    .Text(Fmt(totalHours))
                                                    .Bold().FontSize(22).FontColor("DC2626");
                                                dc.Item().AlignCenter()
                                                    .Text("ساعة").FontSize(8).FontColor("475569");
                                            });
                                        });
                                    });

                                row.ConstantItem(10);

                                // Grève summary
                                row.RelativeItem()
                                    .Border(1).BorderColor(borderClr)
                                    .Background("FFFFFF").Padding(8)
                                    .Column(c =>
                                    {
                                        c.Item().AlignCenter()
                                            .Text("ومن الإضراب عن العمل ما مجموعه:")
                                            .Bold().FontSize(8);
                                        c.Item().Height(6);
                                        c.Item().Row(r =>
                                        {
                                            r.RelativeItem().AlignCenter().Column(dc =>
                                            {
                                                dc.Item().AlignCenter()
                                                    .Text(greveDays.ToString())
                                                    .Bold().FontSize(22).FontColor("7C3AED");
                                                dc.Item().AlignCenter()
                                                    .Text("يوماً").FontSize(8).FontColor("475569");
                                            });
                                            r.ConstantItem(1).Background(borderClr);
                                            r.RelativeItem().AlignCenter().Column(dc =>
                                            {
                                                dc.Item().AlignCenter()
                                                    .Text(Fmt(greveHours))
                                                    .Bold().FontSize(22).FontColor("7C3AED");
                                                dc.Item().AlignCenter()
                                                    .Text("ساعة").FontSize(8).FontColor("475569");
                                            });
                                        });
                                    });
                            });
                        });
                });
            });
        }).GeneratePdf(outputPath);
    }
}