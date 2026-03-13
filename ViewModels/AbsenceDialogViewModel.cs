using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyAvaloniaApp.Models;
using MyAvaloniaApp.Services;

namespace MyAvaloniaApp.ViewModels;

public partial class AbsenceDialogViewModel : ViewModelBase
{
    private readonly AbsenceService _service = new();

    // ── Personnel list ────────────────────────────────────────────────────────
    public ObservableCollection<Personnel> PersonnelList { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedPersonnel))]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private Personnel? _selectedPersonnel;

    public bool HasSelectedPersonnel => SelectedPersonnel is not null;

    // ── Form fields ───────────────────────────────────────────────────────────

    // CalendarDatePicker.SelectedDate is DateTime? in Avalonia.
    // We use DateTime.SpecifyKind(..., Utc) before saving to satisfy Npgsql.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private DateTime? _absenceDate = DateTime.Today;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private string _hoursText = "1";

    [ObservableProperty] private string _selectedAbsenceType = "غياب بدون مبرر";
    [ObservableProperty] private string _notes = string.Empty;

    public ObservableCollection<string> AbsenceTypes { get; } = new()
    {
        "غياب بدون مبرر",
        "رخصة مرض",
        "رخصة إدارية",
        "رخصة الحج",
        "رخصة بدون أجر",
        "رخصة أمومة",
    };

    // ── Absence history ───────────────────────────────────────────────────────
    public ObservableCollection<Absence> AbsenceHistory { get; } = new();
    [ObservableProperty] private bool _isLoadingHistory = false;

    // ── Edit mode ─────────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    [NotifyPropertyChangedFor(nameof(SubmitLabel))]
    private Absence? _editingAbsence;

    public bool   IsEditMode  => EditingAbsence is not null;
    public string SubmitLabel => IsEditMode ? "تحديث" : "تسجيل الغياب";

    // ── Status ────────────────────────────────────────────────────────────────
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool   _isSuccess     = false;
    [ObservableProperty] private bool   _isBusy        = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    public AbsenceDialogViewModel()
    {
        _ = LoadPersonnelAsync();
    }

    private async Task LoadPersonnelAsync()
    {
        var list = await _service.GetAllPersonnelAsync();
        PersonnelList.Clear();
        foreach (var p in list)
            PersonnelList.Add(p);
    }

    partial void OnSelectedPersonnelChanged(Personnel? value)
    {
        ClearForm();
        if (value is not null)
            _ = LoadHistoryAsync(value.Ppr);
        else
            AbsenceHistory.Clear();
    }

    private async Task LoadHistoryAsync(int ppr)
    {
        IsLoadingHistory = true;
        var history = await _service.GetAbsencesForPersonnelAsync(ppr);
        AbsenceHistory.Clear();
        foreach (var a in history)
            AbsenceHistory.Add(a);
        IsLoadingHistory = false;
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        if (!decimal.TryParse(HoursText.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var hours) || hours <= 0)
        {
            StatusMessage = "يرجى إدخال عدد ساعات صحيح";
            IsSuccess     = false;
            return;
        }

        IsBusy        = true;
        StatusMessage = string.Empty;

        // Convert DateTimeOffset? → DateTime (UTC date only, no time component)
        var chosenDate = DateTime.SpecifyKind(AbsenceDate!.Value.Date, DateTimeKind.Utc);

        try
        {
            if (IsEditMode)
            {
                EditingAbsence!.AbsenceDate = chosenDate;
                EditingAbsence!.Hours       = hours;
                EditingAbsence!.AbsenceType = SelectedAbsenceType;
                EditingAbsence!.Notes       = string.IsNullOrWhiteSpace(Notes) ? null : Notes;
                await _service.UpdateAbsenceAsync(EditingAbsence!);
                StatusMessage = "تم التحديث بنجاح ✓";
            }
            else
            {
                var absence = new Absence
                {
                    Ppr         = SelectedPersonnel!.Ppr,
                    AbsenceDate = chosenDate,
                    Hours       = hours,
                    AbsenceType = SelectedAbsenceType,
                    Notes       = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                    CreatedAt   = DateTime.UtcNow,
                };
                await _service.AddAbsenceAsync(absence);
                StatusMessage = "تم تسجيل الغياب بنجاح ✓";
            }

            IsSuccess = true;
            ClearForm();
            await LoadHistoryAsync(SelectedPersonnel!.Ppr);
        }
        catch (Exception ex)
        {
            StatusMessage = $"خطأ: {ex.Message}";
            IsSuccess     = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSubmit() =>
        SelectedPersonnel is not null &&
        !string.IsNullOrWhiteSpace(HoursText) &&
        AbsenceDate is not null;

    [RelayCommand]
    private void EditAbsence(Absence absence)
    {
        EditingAbsence      = absence;
        AbsenceDate         = absence.AbsenceDate;
        HoursText           = absence.Hours.ToString("G");
        SelectedAbsenceType = absence.AbsenceType;
        Notes               = absence.Notes ?? string.Empty;
        StatusMessage       = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteAbsenceAsync(Absence absence)
    {
        await _service.DeleteAbsenceAsync(absence.Id);
        AbsenceHistory.Remove(absence);
        if (EditingAbsence?.Id == absence.Id)
            ClearForm();
        StatusMessage = "تم الحذف ✓";
        IsSuccess     = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        ClearForm();
        StatusMessage = string.Empty;
    }

    private void ClearForm()
    {
        EditingAbsence      = null;
        AbsenceDate         = DateTime.Today;
        HoursText           = "1";
        SelectedAbsenceType = "غياب بدون مبرر";
        Notes               = string.Empty;
    }
}