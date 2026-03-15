using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MyAvaloniaApp.Services;
using MyAvaloniaApp.ViewModels;
using MyAvaloniaApp.Views;

namespace MyAvaloniaApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Always show main window first
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            desktop.MainWindow = mainWindow;

            // Then check if first run and show wizard on top
            _ = CheckFirstRunAsync(mainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async System.Threading.Tasks.Task CheckFirstRunAsync(MainWindow mainWindow)
    {
        var settingsService = new SettingsService();
        var isFirstRun      = await settingsService.IsFirstRunAsync();

        if (isFirstRun)
        {
            var wizard = new SetupWizard();
            await wizard.ShowDialog(mainWindow);
        }
    }
}