using System.Text.Json.Serialization;

namespace FontAwesome.KitToJson.Models;

internal class FontAwesomeIcon
{
    [JsonPropertyName("svg")]
    public Dictionary<string, Svg> Svg { get; set; } = [];
}
