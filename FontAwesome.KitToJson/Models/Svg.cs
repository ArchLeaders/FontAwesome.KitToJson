using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FontAwesome.KitToJson.Models;

internal class Svg
{
    public required JsonElement Path { get; set; }
    public required int[] ViewBox { get; set; }

    [SetsRequiredMembers]
    public Svg(string path, (int X, int Y, int W, int H) viewBox)
    {
        Path = JsonElement.Parse($"\"{path}\"");
        ViewBox = [
            viewBox.X,
            viewBox.Y,
            viewBox.W,
            viewBox.H
        ];
    }

    [SetsRequiredMembers]
    public Svg(string primary, string secondary, (int X, int Y, int W, int H) viewBox)
    {
        Path = JsonElement.Parse($"[\"{primary}\",\"{secondary}\"]");
        ViewBox = [
            viewBox.X,
            viewBox.Y,
            viewBox.W,
            viewBox.H
        ];
    }
}
