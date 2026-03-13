using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyAvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    // ── Header Info ───────────────────────────────────────────────────────────
    [ObservableProperty] private string _academyName     = "سوس ماسة";
    [ObservableProperty] private string _regionName      = "انزكان ايت ملول";
    [ObservableProperty] private string _institutionName = "ثانوية الإمام الجزولي الاعدادية";
    [ObservableProperty] private string _userName        = "محمد ايت دحمانت";
    [ObservableProperty] private string _academicYear    = "2025/2026";

    public string TodayDate { get; } =
        DateTime.Now.ToString("dddd d MMMM yyyy", new CultureInfo("fr-FR"));

    // ── Employee Stats ────────────────────────────────────────────────────────
    [ObservableProperty] private int _totalEmployees = 36;
    [ObservableProperty] private int _maleCount      = 25;
    [ObservableProperty] private int _femaleCount    = 11;
    [ObservableProperty] private int _teacherCount   = 31;
    [ObservableProperty] private int _adminCount     = 5;

    // ── Absence Stats ─────────────────────────────────────────────────────────
    [ObservableProperty] private int _unjustifiedAbsences = 0;
    [ObservableProperty] private int _unpaidLeave         = 0;
    [ObservableProperty] private int _maternityLeave      = 0;
    [ObservableProperty] private int _adminLeave          = 0;
    [ObservableProperty] private int _sickLeave           = 0;
    [ObservableProperty] private int _hajjLeave           = 0;

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Raised by the command so the View can open the import dialog
    /// (dialogs need a Window parent, so we keep them in the View layer).
    /// </summary>
    public event Action? OpenImportDialogRequested;

    [RelayCommand]
    private void OpenImportDialog() => OpenImportDialogRequested?.Invoke();
}