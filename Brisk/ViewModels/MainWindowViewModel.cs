using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;

namespace Brisk.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _currentFile = "new.swift";
    private string _status = "Ready";
    private int _caretIndex;

    public string CurrentFile
    {
        get => _currentFile;
        set => this.RaiseAndSetIfChanged(ref _currentFile, value);
    }
    
    public string Status
    {
        get => _status;
        private set
        {
            this.RaiseAndSetIfChanged(ref _status, value);
            this.RaisePropertyChanged(nameof(StatusColor));
            this.RaisePropertyChanged(nameof(IsRunEnabled)); 
        }
    }

    public IBrush StatusColor
    {
        get
        {
            if (Status == "Ready")
                return Brushes.Green;
            else if (Status == "Error")
                return Brushes.Red;
            else
                return Brushes.Yellow;
        }
    }
    
    public bool IsRunEnabled => Status == "Ready";
    
    public string CaretPosition => $"0:{CaretIndex}";
    public int CaretIndex
    {
        get => _caretIndex;
        set => this.RaiseAndSetIfChanged(ref _caretIndex, value);
    }
    
    public string Script { get; set; } = """
                                        // Swift "Hello, World!" 
                                        Program print("Hello, World!") 
                                        """;

    public ICommand RunScript { get; }
    
    public MainWindowViewModel()
    {
        RunScript = ReactiveCommand.CreateFromTask(async () =>
        {
            Status = "Running";
            await RunScriptAsync(Script);
            Status = "Ready";
        });
    }
    
    private async Task RunScriptAsync(string script)
    {
        await Task.Delay(1000);
    }
}
