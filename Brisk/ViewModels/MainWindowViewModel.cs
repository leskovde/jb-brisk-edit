using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;

namespace Brisk.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _currentFile = "new.swift";
    private string _status = "Ready";
    private int _caretIndex;
    private int _scriptRunCount = 1;
    private Task<int> _currentScriptExecution;
    
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

    public int ScriptRunCount
    {
        get => _scriptRunCount;
        set => this.RaiseAndSetIfChanged(ref _scriptRunCount, value);
    }
    
    public ICommand RunScript { get; }
    public ICommand RunScriptMultipleTimes { get; }
    // public ICommand StopScript { get; }
    public ICommand NewFile { get; }
    public ICommand SaveFile { get; }

    public MainWindowViewModel()
    {
        RunScript = ReactiveCommand.CreateFromTask(async () =>
        {
            Status = "Running";
            _currentScriptExecution = RunScriptAsync(Script);
            await _currentScriptExecution;
            Status = "Ready";
        });
        
        RunScriptMultipleTimes = ReactiveCommand.CreateFromTask(async () =>
        {
            Status = "Running";
            
            // TODO: Set up the progress bar.
            
            for (int i = 0; i < ScriptRunCount; ++i)
            { 
                _currentScriptExecution = RunScriptAsync(Script);
                await _currentScriptExecution;
            }
            
            Status = "Ready";
        });
        
        // StopScript = ReactiveCommand.Create(() =>
        // {
        //     Status = "Ready";
        // });
    }
    
    private async Task<int> RunScriptAsync(string script)
    {
        await Task.Delay(5000);
        return 0;
    }
}
