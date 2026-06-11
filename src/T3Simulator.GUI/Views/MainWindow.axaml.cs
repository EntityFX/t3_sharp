using Avalonia.Controls;

namespace T3Simulator.GUI.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        this.Closed += (s, e) => Instance = null;
    }
}
