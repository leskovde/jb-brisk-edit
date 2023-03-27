using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Brisk.Models;
using ReactiveUI;

namespace Brisk.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private const string _defaultScript = """
                                        // Swift "Hello, World!" Program
                                        print("Hello, World!") 
                                        """;

    private const string _newFileName = "new.swift";
    private const string _readyStatus = "Ready";
    private const string _runningStatus = "Running";
    private const string _errorStatus = "Error";

    private string _status = _readyStatus;
    private int _caretIndex;
    private int _scriptRunCount = 10;
    private int _scriptRunCountBackup;
    private bool _isProgressBarVisible = false;
    private bool _isProgressBarIndeterminate = false;
    private int _currentTabIndex = 0;
    private int _completedTasksCount = 0;
    private Task<int> _currentScriptExecution;
    private string _outputContent = string.Empty;
    private CancellationTokenSource _tokenSource = new();

    public string CurrentFile
    {
        get => OpenTabs[CurrentTabIndex].Header;
    }

    public string Status
    {
        get => _status;
        private set
        {
            this.RaiseAndSetIfChanged(ref _status, value);
            this.RaisePropertyChanged(nameof(StatusColor));
            this.RaisePropertyChanged(nameof(IsRunEnabled));
            this.RaisePropertyChanged(nameof(IsStopEnabled));
        }
    }

    public IBrush StatusColor
    {
        get
        {
            if (Status == _readyStatus)
                return Brushes.Lime;

            if (Status == _errorStatus)
                return Brushes.Red;

            return Brushes.Yellow;
        }
    }

    public bool IsRunEnabled => Status != _runningStatus;
    public bool IsStopEnabled => !IsRunEnabled;

    public string CaretPosition => $"0:{CaretIndex}";

    public int CaretIndex
    {
        get => _caretIndex;
        set => this.RaiseAndSetIfChanged(ref _caretIndex, value);
    }

    public string Script
    {
        get => OpenTabs[CurrentTabIndex].Content;
        set
        {
            OpenTabs[CurrentTabIndex].Content = value;
            OpenTabs[CurrentTabIndex].Dirty = true;
            this.RaisePropertyChanged(nameof(Script));
        }
    }

    public string OutputContent
    {
        get => _outputContent;
        set => this.RaiseAndSetIfChanged(ref _outputContent, value);
    }

    public int ScriptRunCount
    {
        get => _scriptRunCount;
        set => this.RaiseAndSetIfChanged(ref _scriptRunCount, value);
    }

    public bool IsProgressBarVisible
    {
        get => _isProgressBarVisible;
        set => this.RaiseAndSetIfChanged(ref _isProgressBarVisible, value);
    }

    public bool IsProgressbarIndeterminate
    {
        get => _isProgressBarIndeterminate;
        set => this.RaiseAndSetIfChanged(ref _isProgressBarIndeterminate, value);
    }

    public int CompletedTasksCount
    {
        get => _completedTasksCount;
        set => this.RaiseAndSetIfChanged(ref _completedTasksCount, value);
    }

    public ObservableCollection<TabItemModel> OpenTabs { get; } = new()
    {
        new("new1.swift", "Program print(\"Hello, World!\")"),
        new("new2.swift", "Error output goes here.")
    };

    public int CurrentTabIndex
    {
        get => _currentTabIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentTabIndex, value);
            this.RaisePropertyChanged(nameof(CurrentFile));
            this.RaisePropertyChanged(nameof(Script));
        }
    }

    public ICommand RunScript { get; }
    public ICommand RunScriptMultipleTimes { get; }
    public ICommand StopScript { get; }
    public ICommand NewFile { get; }
    public ICommand SaveFile { get; }
    public ICommand Exit { get; }
    public ICommand ShowSettings { get; }
    public Interaction<SettingsViewModel, SettingsViewModel?> ShowSettingsWindow { get; }

    public MainWindowViewModel()
    {
        ShowSettingsWindow = new();

        RunScript = ReactiveCommand.CreateFromTask(async () =>
        {
            _scriptRunCountBackup = 1;
            await RunNTimes();
        });

        RunScriptMultipleTimes = ReactiveCommand.CreateFromTask(async () =>
        {
            _scriptRunCountBackup = ScriptRunCount;
            await RunNTimes();
        });

        StopScript = ReactiveCommand.Create(() => { _tokenSource.Cancel(); });

        NewFile = ReactiveCommand.Create(() =>
        {
            // TODO: Check if the new file already exists and change the name if needed.
            OpenTabs.Add(new TabItemModel(_newFileName, _defaultScript));
            CurrentTabIndex = OpenTabs.Count - 1;
        });

        SaveFile = ReactiveCommand.Create(async () => { await SaveAsync(); });

        Exit = ReactiveCommand.Create(() => { Environment.Exit(0); });
        
        ShowSettings = ReactiveCommand.Create(async () =>
        {
            SettingsViewModel settings = new SettingsViewModel();
            await ShowSettingsWindow.Handle(settings);
        });
    }

    private async Task SaveAsync()
    {
        int savedTabIndex = CurrentTabIndex;
        await File.WriteAllTextAsync(CurrentFile, Script);
        OpenTabs[savedTabIndex].Dirty = false;
    }

    private async Task RunNTimes()
    {
        Status = _runningStatus;
        OutputContent = string.Empty;

        // Save the file first.
        await SaveAsync();

        IsProgressBarVisible = true;
        IsProgressbarIndeterminate = _scriptRunCountBackup == 1;

        for (CompletedTasksCount = 0; CompletedTasksCount < _scriptRunCountBackup; ++CompletedTasksCount)
        {
            _currentScriptExecution = RunScriptAsync();
            int result = await _currentScriptExecution;

            if (result == -1)
            {
                // The task was cancelled.
                await Task.Delay(100);
                OutputContent += "[BRISK]: The script execution was canceled.\n";
                break;
            }
        }

        await Task.Delay(100);
        IsProgressBarVisible = false;
        Status = _readyStatus;
    }

    private async Task<int> RunScriptAsync()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Settings.Instance.SwiftPath,
                Arguments = CurrentFile,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += OutputHandler;
        process.ErrorDataReceived += ErrorHandler;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(_tokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            _tokenSource = new CancellationTokenSource();
            return -1;
        }
        catch (Exception e)
        {
            OutputContent += $"[ERROR (Internal)]: {e.Message}\n";
            _tokenSource = new CancellationTokenSource();
            return -1;
        }

        OutputContent += $"[BRISK]: Script finished with exit code {process.ExitCode}.\n";

        return process.ExitCode;
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        OutputContent += $"{outLine.Data}\n";
    }

    private void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
            OutputContent += $"[ERROR]: {outLine.Data}\n";
    }
}
