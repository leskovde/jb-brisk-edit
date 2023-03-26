using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Brisk.Views;

public partial class ToolBar : UserControl
{
    public ToolBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

