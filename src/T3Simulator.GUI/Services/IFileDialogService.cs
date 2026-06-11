using System.Threading.Tasks;

namespace T3Simulator.GUI.Services;

public interface IFileDialogService
{
    Task<string?> OpenFileAsync(string filter);
    Task<string?> SaveFileAsync(string filter, string defaultFileName);
}