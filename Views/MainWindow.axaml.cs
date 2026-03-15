using Avalonia.Controls;
using MyAvaloniaApp.ViewModels;

namespace MyAvaloniaApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.OpenImportDialogRequested += () =>
                {
                    var dialog = new ImportDialog();
                    dialog.ShowDialog(this);
                };

                vm.OpenAbsenceDialogRequested += () =>
                {
                    var dialog = new AbsenceDialog();
                    dialog.ShowDialog(this);
                };

                vm.OpenAbsenceCardDialogRequested += (institutionName, academicYear) =>
                {
                    var dialog = new AbsenceCardDialog(institutionName, academicYear);
                    dialog.ShowDialog(this);
                };
            }
        };
    }
}