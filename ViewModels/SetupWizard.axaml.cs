using Avalonia.Controls;
using MyAvaloniaApp.ViewModels;

namespace MyAvaloniaApp.Views;

public partial class SetupWizard : Window
{
    public SetupWizard()
    {
        InitializeComponent();
        var vm = new SetupWizardViewModel();
        vm.SetupCompleted += () => Close();
        DataContext = vm;
    }
}