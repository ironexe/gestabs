using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyAvaloniaApp.Models;
using MyAvaloniaApp.Services;

namespace MyAvaloniaApp.ViewModels;

public partial class AbsenceCardDialogViewModel : ViewModelBase
{
    private readonly AbsenceService     _absenceService = new();
    private readonly AbsenceCardService _cardService    = new();

    // ── Settings passed from main window ─────────────────────────────────────
    public string InstitutionName { get; set; } = string.Empty;
    public string AcademicYear    { get; set; } = string.Empty;

    // ── Personnel list ────────────────────────────────────────────────────────
    public ObservableCollection<Personnel> PersonnelList { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedPersonnel))]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    private Personnel? _selectedPersonnel;

    public bool HasSelectedPersonnel => SelectedPersonnel is not null;

    // ── State ─────────────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _isBusy        = false;
    [ObservableProperty] private bool   _isSuccess     = false;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private string _outputPath    = string.Empty;

    // Raised so the View can open a SaveFileDialog (needs Window reference)
    public event Func<Task<string?>>? PickSavePathRequested;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    public AbsenceCardDialogViewModel()
    {
        _ = LoadPersonnelAsync();
    }

    private async Task LoadPersonnelAsync()
    {
        var list = await _absenceService.GetAllPersonnelAsync();
        PersonnelList.Clear();
        foreach (var p in list)
            PersonnelList.Add(p);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateAsync()
    {
        // Ask the View for a save path
        var path = PickSavePathRequested is not null
            ? await PickSavePathRequested.Invoke()
            : null;

        if (string.IsNullOrWhiteSpace(path)) return;

        IsBusy        = true;
        IsSuccess     = false;
        StatusMessage = "جاري إنشاء البطاقة...";

        try
        {
            var absences = await _absenceService
                .GetAbsencesForPersonnelAsync(SelectedPersonnel!.Ppr);

            await Task.Run(() =>
                _cardService.Generate(
                    SelectedPersonnel!,
                    absences,
                    InstitutionName,
                    AcademicYear,
                    path));

            OutputPath    = path;
            IsSuccess     = true;
            StatusMessage = "تم إنشاء البطاقة بنجاح ✓";
        }
        catch (Exception ex)
        {
            IsSuccess     = false;
            StatusMessage = $"خطأ: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGenerate() => SelectedPersonnel is not null && !IsBusy;
}