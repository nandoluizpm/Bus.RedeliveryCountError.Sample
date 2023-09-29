using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bus.RedeliveryCountError.Sample.Extensions;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions ConfigureSettings(this JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
        options.DictionaryKeyPolicy = new SnakeCaseNamingPolicy();
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.WriteIndented = false;
        
        return options;
    }
}

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public static SnakeCaseNamingPolicy Instance { get; } = new();

    public override string ConvertName(string name) => name.ToSnakeCase();
}