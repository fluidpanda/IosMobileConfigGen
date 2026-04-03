namespace IosMobileConfigGen;

/// <summary>
/// Builds inner VPN payload dictionary for a specific VPN types
/// </summary>
public interface IVpnPayloadBuilder
{
    Dictionary<string, object> Build(VpnProfileConfig config, string payloadUuid);
}
