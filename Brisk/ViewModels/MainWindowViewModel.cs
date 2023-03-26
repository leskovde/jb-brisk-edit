using Avalonia.Media;
using ReactiveUI;

namespace Brisk.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string CurrentFile => "new.txt";
    public string Status => "Ready";
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
    public string CaretPosition => $"0:{CaretIndex}";

    private int _caretIndex;
    public int CaretIndex
    {
        get => _caretIndex;
        set => this.RaiseAndSetIfChanged(ref _caretIndex, value);
    }
}
