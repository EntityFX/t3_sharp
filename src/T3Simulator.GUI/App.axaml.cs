using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using T3Simulator.GUI.ViewModels;
using T3Simulator.GUI.Views;
using T3Simulator.GUI.Services;

namespace T3Simulator.GUI;

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
            var fileDialogService = new AvaloniaFileDialogService();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(fileDialogService),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}