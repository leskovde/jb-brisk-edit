using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Brisk.Views;

public partial class OutputWindow : UserControl
{
    public OutputWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

