using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using T3Simulator.GUI.Views;

namespace T3Simulator.GUI.Services;

public class AvaloniaFileDialogService : IFileDialogService
{
    /// <summary>
    /// Opens a file dialog to select a file.
    /// Uses Dispatcher.UIThread.InvokeAsync to ensure the dialog runs on the STA thread.
    /// </summary>
    public async Task<string?> OpenFileAsync(string filter)
    {
        return await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                // System.Windows.Forms dialogs MUST be created and shown on an STA thread.
                // Avalonia's main thread is STA, so we use Dispatcher.UIThread.
                using var dialog = new System.Windows.Forms.OpenFileDialog
                {
                    Filter = filter,
                    Title = "Open Ternary Program"
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.FileName;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening file: {ex.Message}");
            }
            return null;
        });
    }

    /// <summary>
    /// Opens a file dialog to save a file.
    /// Uses Dispatcher.UIThread.InvokeAsync to ensure the dialog runs on the STA thread.
    /// </summary>
    public async Task<string?> SaveFileAsync(string filter, string defaultFileName)
    {
        return await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                using var dialog = new System.Windows.Forms.SaveFileDialog
                {
                    Filter = filter,
                    FileName = defaultFileName,
                    Title = "Save Execution Log"
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.FileName;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving file: {ex.Message}");
            }
            return null;
        });
    }
}