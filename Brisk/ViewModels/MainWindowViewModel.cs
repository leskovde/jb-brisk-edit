﻿using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using Brisk.Models;
using ReactiveUI;

namespace Brisk.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private const string _defaultScript = """
                                        // Swift "Hello, World!" 
                                        Program print("Hello, World!") 
                                        """;

    private const string _newFileName = "new.swift";
    private const string _readyStatus = "Ready";
    private const string _runningStatus = "Running";
    private const string _errorStatus = "Error";

    private string _currentFile = _newFileName;
    private string _status = _readyStatus;
    private int _caretIndex;
    private int _scriptRunCount = 5;
    private bool _isProgressBarVisible = false;
    private bool _isProgressBarIndeterminate = false;
    private int _currentTabIndex = 0;
    private int _completedTasksCount = 0;
    private Task<int> _currentScriptExecution;

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
                return Brushes.Green;
            else if (Status == _errorStatus)
                return Brushes.Red;
            else
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
            this.RaisePropertyChanged(nameof(Script));
        }
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

    public List<TabItemModel> OpenTabs { get; set; } = new()
    {
        new("new1.swift", "Console output goes here."),
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

    // public ICommand StopScript { get; }
    public ICommand NewFile { get; }
    public ICommand SaveFile { get; }
    public ICommand ShowSettings { get; }
    public Interaction<SettingsViewModel, SettingsViewModel?> ShowSettingsWindow { get; }

    public MainWindowViewModel()
    {
        ShowSettingsWindow = new ();
        
        RunScript = ReactiveCommand.CreateFromTask(async () =>
        {
            Status = _runningStatus;

            // Save the file first.
            await SaveAsync();

            IsProgressBarVisible = true;
            IsProgressbarIndeterminate = true;

            _currentScriptExecution = RunScriptAsync(Script);
            await _currentScriptExecution;

            IsProgressBarVisible = false;

            Status = _readyStatus;
        });

        RunScriptMultipleTimes = ReactiveCommand.CreateFromTask(async () =>
        {
            Status = _runningStatus;

            // Save the file first.
            await SaveAsync();

            IsProgressBarVisible = true;
            IsProgressbarIndeterminate = false;

            for (CompletedTasksCount = 0; CompletedTasksCount < ScriptRunCount; ++CompletedTasksCount)
            {
                _currentScriptExecution = RunScriptAsync(Script);
                await _currentScriptExecution;
            }

            IsProgressBarVisible = false;
            Status = _readyStatus;
        });

        // StopScript = ReactiveCommand.Create(() =>
        // {
        //     Status = "Ready";
        // });

        NewFile = ReactiveCommand.Create(() =>
        {
            // TODO: Check if the new file already exists and change the name if needed.
            OpenTabs.Add(new TabItemModel(_newFileName, _defaultScript));
            CurrentTabIndex = OpenTabs.Count - 1;
        });

        SaveFile = ReactiveCommand.Create(async () => { await SaveAsync(); });
        
        ShowSettings = ReactiveCommand.Create(async () =>
        {
            SettingsViewModel settings = new SettingsViewModel();
            var dialog = await ShowSettingsWindow.Handle(settings);
        });
    }

    private async Task SaveAsync()
    {
        await File.WriteAllTextAsync(CurrentFile, Script);
    }

    private async Task<int> RunScriptAsync(string script)
    {
        string command = $"/bin/bash -c \"echo '{script}' | {Settings.Instance.SwiftPath}\"";

        await Task.Delay(5000);
        return 0;
    }
}
