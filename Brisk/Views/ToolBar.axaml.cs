using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Brisk.Models;
using Brisk.ViewModels;

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

    public async void OpenFile_Clicked(object sender, RoutedEventArgs args)
    {
        string path = await GetPath();
        
        if (string.IsNullOrEmpty(path))
            return;

        MainWindowViewModel? context = DataContext as MainWindowViewModel;
        context.OpenTabs.Add(new TabItemModel(path));
    }
    
    private async Task<string> GetPath()
    {
        OpenFileDialog dialog = new()
        {
            AllowMultiple = false,
            Directory = Directory.GetCurrentDirectory()
        };

        string[] result = await dialog.ShowAsync(Parent?.Parent as Window);

        return result?.FirstOrDefault();
    }
}

