using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyAvaloniaApp.Services;

namespace MyAvaloniaApp.ViewModels;

public partial class ImportDialogViewModel : ViewModelBase
{
    private readonly XmlImportService _service = new();

    // ── State ─────────────────────────────────────────────────────────────────

    [ObservableProperty] private string  _filePath    = string.Empty;
    [ObservableProperty] private bool    _isImporting = false;
    [ObservableProperty] private bool    _isDone      = false;
    [ObservableProperty] private string  _statusMessage = string.Empty;
    [ObservableProperty] private bool    _hasError    = false;

    // Summary counts (shown after import)
    [ObservableProperty] private int _insertedCount;
    [ObservableProperty] private int _updatedCount;
    [ObservableProperty] private int _skippedCount;

    public bool CanImport => !string.IsNullOrWhiteSpace(FilePath) && !IsImporting;

    partial void OnFilePathChanged(string value)    => OnPropertyChanged(nameof(CanImport));
    partial void OnIsImportingChanged(bool value)   => OnPropertyChanged(nameof(CanImport));

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task RunImportAsync()
    {
        IsImporting    = true;
        IsDone         = false;
        HasError       = false;
        StatusMessage  = "جاري استيراد البيانات...";

        var result = await Task.Run(() => _service.ImportAsync(FilePath));

        IsImporting = false;
        IsDone      = true;

        if (result.ErrorMessage is not null)
        {
            HasError      = true;
            StatusMessage = $"خطأ: {result.ErrorMessage}";
        }
        else
        {
            HasError       = false;
            InsertedCount  = result.Inserted;
            UpdatedCount   = result.Updated;
            SkippedCount   = result.Skipped;
            StatusMessage  = "تم الاستيراد بنجاح ✓";
        }
    }
}