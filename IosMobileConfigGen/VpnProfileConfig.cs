using System.Text.Json.Serialization;

namespace IosMobileConfigGen;

/// <summary>
/// Root configuration for VPN profile generation.
/// Deserializable from JSON config file or constructable from cli arguments.
/// </summary>
public record VpnProfileConfig
{
    [JsonPropertyName("name")] 
    public required string Name { get; init; }
    
    [JsonPropertyName("server")] 
    public required string Server { get; init; }
    
    [JsonPropertyName("username")] 
    public required string UserName { get; init; }
    
    [JsonPropertyName("password")] 
    public string? Password { get; init; } // might be omitted in config
    
    [JsonPropertyName("sharedSecret")] 
    public required string SharedSecret { get; init; }
    
    [JsonPropertyName("organization")] 
    public string Organization { get; init; } = "Undefined";
    
    [JsonPropertyName("identifier")] 
    public string Identifier { get; init; } = "com.example.vpn.endpoint";
    
    [JsonPropertyName("sendAllTraffic")] 
    public bool SendAllTraffic { get; init; } = true;
    
    [JsonPropertyName("onDemand")]
    public OnDemandConfig? OnDemand { get; init; }
    
    [JsonPropertyName("output")]
    public string Output { get; init; } = "vpn.mobileconfig";
}

public record OnDemandConfig
{
    [JsonPropertyName("mode")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OnDemandMode Mode { get; init; } = OnDemandMode.WiFiAndCellular;

    [JsonPropertyName("onDemandMode")] 
    public string[] ExcludedSsids { get; init; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OnDemandMode
{
    Disabled, // no auto-connect
    WiFiOnly, // only on wifi, disconnect on cellular
    WiFiAndCellular  // auto-connect on wifi and cellular 
}
