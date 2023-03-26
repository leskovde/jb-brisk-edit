using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Brisk.Views;

public partial class ActionBar : UserControl
{
    public ActionBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

