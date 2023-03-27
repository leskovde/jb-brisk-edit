namespace Brisk.Models;

public class TabItemModel
{
    public string Header { get; }
    public string Content;

    public TabItemModel(string header, string content)
    {
        Header = header;
        Content = content;
    }
}
