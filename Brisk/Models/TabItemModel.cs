using System.IO;
using ReactiveUI;

namespace Brisk.Models;

public class TabItemModel : ReactiveObject
{
    private bool _dirty = false;
    
    public string Header { get; }
    public string Content;

    public bool Dirty
    {
        get => _dirty;
        set => this.RaiseAndSetIfChanged(ref _dirty, value);
    }

    public TabItemModel(string header, string content)
    {
        Header = header;
        Content = content;
    }

    public TabItemModel(string path)
    {
        Header = Path.GetFileName(path);
        Content = File.ReadAllText(path);
    }
}
