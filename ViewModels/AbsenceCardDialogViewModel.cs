using System;
using System.Collections.ObjectModel;
using System.Linq;
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

    public string InstitutionName { get; set; } = string.Empty;
    public string AcademicYear    { get; set; } = string.Empty;

    // ── Personnel list ────────────────────────────────────────────────────────
    public ObservableCollection<Personnel> PersonnelList { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedPersonnel))]
    [NotifyCanExecuteChangedFor(nameof(GenerateSingleCommand))]
    private Personnel? _selectedPersonnel;

    public bool HasSelectedPersonnel => SelectedPersonnel is not null;

    // ── State ─────────────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _isBusy        = false;
    [ObservableProperty] private bool   _isSuccess     = false;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private string _outputPath    = string.Empty;

    public event Func<string, Task<string?>>? PickSavePathRequested;

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

    // ── Single employee (landscape) ───────────────────────────────────────────
    [RelayCommand(CanExecute = nameof(CanGenerateSingle))]
    private async Task GenerateSingleAsync()
    {
        var suggested = $"بطاقة_غياب_{SelectedPersonnel?.NomLatin}";
        var path = PickSavePathRequested is not null
            ? await PickSavePathRequested.Invoke(suggested) : null;
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
                    SelectedPersonnel!, absences,
                    InstitutionName, AcademicYear, path));

            OutputPath    = path;
            IsSuccess     = true;
            StatusMessage = "تم إنشاء البطاقة بنجاح ✓";
        }
        catch (Exception ex)
        {
            IsSuccess     = false;
            StatusMessage = $"خطأ: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    private bool CanGenerateSingle() => SelectedPersonnel is not null && !IsBusy;

    // ── All employees (portrait, 2 per page) ──────────────────────────────────
    [RelayCommand]
    private async Task GenerateAllAsync()
    {
        var path = PickSavePathRequested is not null
            ? await PickSavePathRequested.Invoke("بطاقات_غياب_جميع_الموظفين") : null;
        if (string.IsNullOrWhiteSpace(path)) return;

        IsBusy        = true;
        IsSuccess     = false;
        StatusMessage = "جاري إنشاء بطاقات جميع الموظفين...";

        try
        {
            // Load all absences for all personnel
            var allData = new System.Collections.Generic.List<(Personnel, System.Collections.Generic.List<Absence>)>();
            foreach (var p in PersonnelList)
            {
                var abs = await _absenceService.GetAbsencesForPersonnelAsync(p.Ppr);
                allData.Add((p, abs));
            }

            await Task.Run(() =>
                _cardService.GenerateAll(allData, InstitutionName, AcademicYear, path));

            OutputPath    = path;
            IsSuccess     = true;
            StatusMessage = $"تم إنشاء {allData.Count} بطاقة بنجاح ✓";
        }
        catch (Exception ex)
        {
            IsSuccess     = false;
            StatusMessage = $"خطأ: {ex.Message}";
        }
        finally { IsBusy = false; }
    }
}