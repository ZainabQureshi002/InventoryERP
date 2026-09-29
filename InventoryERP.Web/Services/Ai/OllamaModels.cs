using System.Text.Json;
using System.Text.Json.Serialization;

namespace InventoryERP.Web.Services.Ai;

public class OllamaChatRequest
{
    public string model { get; set; } = string.Empty;
    public List<OllamaMessage> messages { get; set; } = new();
    public List<object>? tools { get; set; }
    public bool stream { get; set; }
}

public class OllamaMessage
{
    public string role { get; set; } = string.Empty;
    public string? content { get; set; }
    public List<OllamaToolCall>? tool_calls { get; set; }
}

public class OllamaToolCall
{
    public OllamaToolCallFunction function { get; set; } = new();
}

public class OllamaToolCallFunction
{
    public string name { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonElementConverter))]
    public JsonElement arguments { get; set; }
}

public class OllamaChatResponse
{
    public OllamaMessage? message { get; set; }
    public bool done { get; set; }
}

/// <summary>Pass-through converter so `arguments` (a JSON object) can be read as a raw JsonElement.</summary>
public class JsonElementConverter : JsonConverter<JsonElement>
{
    public override JsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonDocument.ParseValue(ref reader).RootElement.Clone();

    public override void Write(Utf8JsonWriter writer, JsonElement value, JsonSerializerOptions options)
        => value.WriteTo(writer);
}
