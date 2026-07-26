using Avalonia.Controls;

namespace Avalonia.Diagnostics.Sample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.AttachDevTools();
    }
}
