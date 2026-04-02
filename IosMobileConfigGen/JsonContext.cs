using System.Text.Json.Serialization;

namespace IosMobileConfigGen;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(VpnProfileConfig))]
internal partial class AppJsonContext : JsonSerializerContext;