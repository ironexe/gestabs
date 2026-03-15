using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MyAvaloniaApp.ViewModels;

namespace MyAvaloniaApp.Views;

public partial class AbsenceCardDialog : Window
{
    public AbsenceCardDialog(string institutionName, string academicYear)
    {
        InitializeComponent();

        var vm = new AbsenceCardDialogViewModel
        {
            InstitutionName = institutionName,
            AcademicYear    = academicYear,
        };

        // Wire up the save file picker
        vm.PickSavePathRequested += async () =>
        {
            var options = new FilePickerSaveOptions
            {
                Title           = "حفظ بطاقة الغياب",
                SuggestedFileName = $"بطاقة_غياب_{vm.SelectedPersonnel?.NomLatin}",
                DefaultExtension  = "pdf",
                FileTypeChoices =
                [
                    new FilePickerFileType("PDF Files") { Patterns = ["*.pdf"] }
                ]
            };

            var file = await StorageProvider.SaveFilePickerAsync(options);
            return file?.Path.LocalPath;
        };

        DataContext = vm;

        var closeBtn = this.FindControl<Button>("CloseButton")!;
        closeBtn.Click += (_, _) => Close();
    }
}