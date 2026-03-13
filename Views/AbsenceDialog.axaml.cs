using Avalonia.Controls;
using MyAvaloniaApp.ViewModels;

namespace MyAvaloniaApp.Views;

public partial class AbsenceDialog : Window
{
    public AbsenceDialog()
    {
        InitializeComponent();
        DataContext = new AbsenceDialogViewModel();
    }
}