using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using T3Simulator.GUI.Views;

namespace T3Simulator.GUI.Services
{
    public class AvaloniaFileDialogService : IFileDialogService
    {
        private static Window? GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return desktop.MainWindow;
            return null;
        }

        public async Task<string?> OpenFileAsync(string filter)
        {
            var window = GetMainWindow();
            if (window == null) return null;

            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Ternary Program",
                AllowMultiple = false
            });

            var file = files?.FirstOrDefault();
            return file?.Path.LocalPath;
        }

        public async Task<string?> SaveFileAsync(string filter, string defaultFileName)
        {
            var window = GetMainWindow();
            if (window == null) return null;

            var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Execution Log",
                SuggestedFileName = defaultFileName
            });

            return file?.Path.LocalPath;
        }
    }
}