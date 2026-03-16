using System.Threading.Tasks;
using Avalonia.Controls;
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

        vm.PickSavePathRequested += async (suggestedName) =>
        {
            var options = new FilePickerSaveOptions
            {
                Title             = "حفظ بطاقة الغياب",
                SuggestedFileName = suggestedName,
                DefaultExtension  = "pdf",
                FileTypeChoices   =
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