using System;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyAvaloniaApp.Models;
using MyAvaloniaApp.Services;

namespace MyAvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SettingsService _settingsService = new();

    // ── Settings (editable, persisted) ───────────────────────────────────────
    [ObservableProperty] private string _institutionName = "ثانوية الإمام الجزولي الاعدادية";
    [ObservableProperty] private string _academyName     = "سوس ماسة";
    [ObservableProperty] private string _regionName      = "انزكان ايت ملول";
    [ObservableProperty] private string _academicYear    = "2025/2026";
    [ObservableProperty] private string _directorName    = "محمد ايت دحمانت";

    // ── Edit mode toggle ──────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotEditing))]
    private bool _isEditing = false;
    public bool IsNotEditing => !IsEditing;

    // ── Date ──────────────────────────────────────────────────────────────────
    public string TodayDate { get; } =
        DateTime.Now.ToString("dddd d MMMM yyyy", new CultureInfo("fr-FR"));

    // ── Employee Stats ────────────────────────────────────────────────────────
    [ObservableProperty] private int _totalEmployees = 0;
    [ObservableProperty] private int _maleCount      = 0;
    [ObservableProperty] private int _femaleCount    = 0;
    [ObservableProperty] private int _teacherCount   = 0;
    [ObservableProperty] private int _adminCount     = 0;

    // ── Absence Stats ─────────────────────────────────────────────────────────
    [ObservableProperty] private int _unjustifiedAbsences = 0;
    [ObservableProperty] private int _unpaidLeave         = 0;
    [ObservableProperty] private int _maternityLeave      = 0;
    [ObservableProperty] private int _adminLeave          = 0;
    [ObservableProperty] private int _sickLeave           = 0;
    [ObservableProperty] private int _hajjLeave           = 0;

    // ── Dialog events ─────────────────────────────────────────────────────────
    public event Action? OpenImportDialogRequested;
    public event Action? OpenAbsenceDialogRequested;
    public event Action<string, string>? OpenAbsenceCardDialogRequested;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    public MainWindowViewModel()
    {
        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var s = await _settingsService.LoadAsync();
        InstitutionName = s.InstitutionName;
        AcademyName     = s.AcademyName;
        RegionName      = s.RegionName;
        AcademicYear    = s.AcademicYear;
        DirectorName    = s.DirectorName;
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void ToggleEdit()
    {
        if (IsEditing)
            _ = SaveSettingsAsync();
        IsEditing = !IsEditing;
    }

    private async Task SaveSettingsAsync()
    {
        await _settingsService.SaveAsync(new AppSettings
        {
            Id              = 1,
            InstitutionName = InstitutionName,
            AcademyName     = AcademyName,
            RegionName      = RegionName,
            AcademicYear    = AcademicYear,
            DirectorName    = DirectorName,
        });
    }

    [RelayCommand]
    private void OpenImportDialog() => OpenImportDialogRequested?.Invoke();

    [RelayCommand]
    private void OpenAbsenceDialog() => OpenAbsenceDialogRequested?.Invoke();

    [RelayCommand]
    private void OpenAbsenceCardDialog() =>
        OpenAbsenceCardDialogRequested?.Invoke(InstitutionName, AcademicYear);
}