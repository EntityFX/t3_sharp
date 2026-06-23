using Avalonia.Controls;
using T3Converter.GUI.ViewModels;

namespace T3Converter.GUI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}