using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Brisk.Models;

public class Settings
{
    public static Settings Instance { get; } = new();
    public string SwiftPath { get; set; }

    private Settings()
    {
        try
        {
            using var stream = new FileStream("settings.json", FileMode.Open);
            var settings = JsonSerializer.Deserialize<Settings>(stream);
            
            SwiftPath = settings != null ? settings.SwiftPath : "/usr/bin/swift";
        }
        catch (Exception)
        {
            SwiftPath = "/usr/bin/swift";
        }
    }

    [JsonConstructor]
    public Settings(string swiftPath)
    {
        SwiftPath = swiftPath;
    }
    
    public void Save()
    {
        using var stream = new FileStream("settings.json", FileMode.Create);
        JsonSerializer.Serialize(stream, this);
    }
}
