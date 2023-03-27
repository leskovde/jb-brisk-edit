using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Brisk.ViewModels;
using ReactiveUI;

namespace Brisk.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.ShowSettingsWindow.RegisterHandler(DoShowDialogAsync)));
    }
    
    private async Task DoShowDialogAsync(InteractionContext<SettingsViewModel, SettingsViewModel?> interaction)
    {
        var dialog = new SettingsWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<SettingsViewModel?>(this);
        interaction.SetOutput(result);
    }
}
