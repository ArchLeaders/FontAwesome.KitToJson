using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Xml;
using ConsoleAppFramework;
using FontAwesome.KitToJson.Models;
using Kokuban;

namespace FontAwesome.KitToJson;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class App
{
    private static readonly JsonSerializerOptions _options = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    /// <summary>Convert a FontAwesome kit to a JSON metadata file.</summary>
    /// <param name="kitFolderPath">The path to an unzipped FontAwesome kit.</param>
    /// <param name="output">-o, Output file path.</param>
    [Command("to-json")]
    public void KitToJson([Argument] string kitFolderPath, string output = "info.json")
    {
        var metadata = new Dictionary<string, FontAwesomeIcon>();
        var icons = Directory.EnumerateDirectories(Path.Combine(kitFolderPath, "svgs"))
            .SelectMany(Directory.EnumerateFiles, (path, file) => (
                Style: Path.GetFileNameWithoutExtension(path),
                Icon: Path.GetFileNameWithoutExtension(file), File: file))
            .GroupBy(icon => icon.Icon, icon => (icon.Style, icon.File));

        foreach (var styles in icons) {
            var icon = metadata[styles.Key] = new FontAwesomeIcon();
            foreach (var (style, file) in styles) {
                if (!ParseSvg(file, out var viewBox, out var primary, out var secondary)) {
                    Console.WriteLine(Chalk.Red + $"[Error] {styles.Key}({style}): Failed to parse SVG.");
                    continue;
                }
                
                icon.Svg[style] = secondary is null
                    ? new Svg(primary, viewBox)
                    : new Svg(primary, secondary, viewBox);
                
                Console.WriteLine($"Parsed Icon: {styles.Key}, Style: {style}");
            }
        }

        using var fs = File.Create(output);
        JsonSerializer.Serialize(fs, metadata, _options);
    }
    
    /// <summary>Convert a FontAwesome kit to a JSON metadata file.</summary>
    /// <param name="jsonFilePath">The path to an unzipped FontAwesome kit.</param>
    /// <param name="output">-o, Output file path.</param>
    [Command("min")]
    public void Strip([Argument] string jsonFilePath, string output = "info.json")
    {
        using var fs = File.OpenRead(jsonFilePath);
        var min = JsonSerializer.Deserialize<Dictionary<string, FontAwesomeIcon>>(fs);

        using var outputFs = File.Create(output);
        JsonSerializer.Serialize(outputFs, min, _options);
    }

    private static bool ParseSvg(string filePath, out (int X, int Y, int W, int H) viewBox, [MaybeNullWhen(false)] out string primary, out string? secondary)
    {
        using var fs = File.OpenRead(filePath);
        using var reader = XmlReader.Create(fs);
        
        reader.ReadToFollowing("svg");
        reader.MoveToAttribute("viewBox");
        int[] viewBoxValues = reader.Value.Split(' ').Select(int.Parse).ToArray();
        viewBox = (
            X: viewBoxValues[0],
            Y: viewBoxValues[1],
            W: viewBoxValues[2],
            H: viewBoxValues[3]
        );

        primary = null;
        secondary = null;
        
        while (reader.ReadToFollowing("path")) {
            if (reader.MoveToAttribute("opacity")) {
                reader.MoveToAttribute("d");
                secondary = reader.Value;
                continue;
            }
            
            reader.MoveToAttribute("d");
            primary = reader.Value;
        }

        return primary is not null;
    }
}