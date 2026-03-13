using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MyAvaloniaApp.ViewModels;

namespace MyAvaloniaApp.Views;

public partial class ImportDialog : Window
{
    public ImportDialog()
    {
        InitializeComponent();
        DataContext = new ImportDialogViewModel();

        // Wire up the browse button (file picker needs the Window reference)
        var browseBtn = this.FindControl<Button>("BrowseButton")!;
        browseBtn.Click += OnBrowseClick;

        var closeBtn = this.FindControl<Button>("CloseButton")!;
        closeBtn.Click += (_, _) => Close();
    }

    private async void OnBrowseClick(object? sender, RoutedEventArgs e)
    {
        var options = new FilePickerOpenOptions
        {
            Title               = "اختر ملف XML",
            AllowMultiple       = false,
            FileTypeFilter      =
            [
                new FilePickerFileType("XML Files") { Patterns = ["*.xml"] },
                new FilePickerFileType("All Files")  { Patterns = ["*.*"]  },
            ]
        };

        var files = await StorageProvider.OpenFilePickerAsync(options);
        if (files.Count > 0 && DataContext is ImportDialogViewModel vm)
            vm.FilePath = files[0].Path.LocalPath;
    }
}