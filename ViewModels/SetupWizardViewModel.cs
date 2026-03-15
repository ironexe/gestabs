using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyAvaloniaApp.Models;
using MyAvaloniaApp.Services;

namespace MyAvaloniaApp.ViewModels;

public partial class SetupWizardViewModel : ViewModelBase
{
    private readonly SettingsService _service = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _institutionName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _academyName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _regionName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _academicYear = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _directorName = string.Empty;

    [ObservableProperty] private bool   _isBusy        = false;
    [ObservableProperty] private string _errorMessage  = string.Empty;

    public event System.Action? SetupCompleted;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsBusy       = true;
        ErrorMessage = string.Empty;
        try
        {
            await _service.SaveAsync(new AppSettings
            {
                Id              = 1,
                InstitutionName = InstitutionName.Trim(),
                AcademyName     = AcademyName.Trim(),
                RegionName      = RegionName.Trim(),
                AcademicYear    = AcademicYear.Trim(),
                DirectorName    = DirectorName.Trim(),
            });
            SetupCompleted?.Invoke();
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"خطأ أثناء الحفظ: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() =>
        !string.IsNullOrWhiteSpace(InstitutionName) &&
        !string.IsNullOrWhiteSpace(AcademyName)     &&
        !string.IsNullOrWhiteSpace(RegionName)       &&
        !string.IsNullOrWhiteSpace(AcademicYear)     &&
        !string.IsNullOrWhiteSpace(DirectorName)     &&
        !IsBusy;
}