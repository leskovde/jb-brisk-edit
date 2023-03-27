using System.Windows.Input;
using Brisk.Models;
using ReactiveUI;

namespace Brisk.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private bool _isSaveEnabled = true;
    
    public bool IsSaveEnabled
    {
        get => _isSaveEnabled;
        set => this.RaiseAndSetIfChanged(ref _isSaveEnabled, value);
    }
    
    public string SwiftPath
    {
        get => Settings.Instance.SwiftPath;
        set
        {
            Settings.Instance.SwiftPath = value;
            this.RaisePropertyChanged(nameof(SwiftPath));
        }
    }

    public ICommand SaveCommand { get; }

    public SettingsViewModel()
    {
        SaveCommand = ReactiveCommand.Create(() =>
        {
            IsSaveEnabled = false;
            Settings.Instance.Save();
            IsSaveEnabled = true;
        });
    }
}
