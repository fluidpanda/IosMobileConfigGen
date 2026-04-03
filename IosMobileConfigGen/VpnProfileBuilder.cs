using System.Text;

namespace IosMobileConfigGen;

/// <summary>
/// Builds an Apple Configuration Profile dictionary tree for L2TP/IPSec VPN.
/// </summary>
public static class VpnProfileBuilder
{
    public static Dictionary<string, object> Build(VpnProfileConfig config)
    {
        var payloadBuilder = CreatePayloadBuilder(config.VpnType?.Type ?? VpnType.L2TP);
        var payloadUuid = Guid.NewGuid().ToString().ToUpperInvariant();
        var vpnPayload = payloadBuilder.Build(config, payloadUuid);
        if (config.OnDemand is { Mode: not OnDemandMode.Disabled } onDemand)
        {
            vpnPayload["OnDemandEnabled"] = 1;
            vpnPayload["OnDemandRules"] = BuildOnDemandRules(onDemand);
        }
        return new Dictionary<string, object>
        {
            ["PayloadContent"] = new List<object> { vpnPayload },
            ["PayloadDisplayName"] = "VPN Configuration",
            ["PayloadIdentifier"] = config.Identifier,
            ["PayloadOrganization"] = config.Organization,
            ["PayloadRemovalDisallowed"] = false,
            ["PayloadType"] = "Configuration",
            ["PayloadUUID"] = Guid.NewGuid().ToString().ToUpperInvariant(),
            ["PayloadVersion"] = 1,
        };
    }

    private static IVpnPayloadBuilder CreatePayloadBuilder(VpnType type) => type switch
    {
        VpnType.L2TP => new L2tpPayloadBuilder(),
        VpnType.IKEv2 => new Ikev2PayloadBuilder(),
        _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported VPN type: {type}")
    };

    private static List<object> BuildOnDemandRules(OnDemandConfig onDemand)
    {
        var rules = new List<object>();
        // disconnect when connected to an excluded SSID:
        if (onDemand.ExcludedSsids.Length > 0)
        {
            rules.Add(new Dictionary<string, object>
            {
                ["Action"] = "Disconnect",
                ["InterfaceTypeMatch"] = "WiFi",
                ["SSIDMatch"] = new List<object>(onDemand.ExcludedSsids),
            });
        }
        // connect on any other Wi-Fi networks:
        rules.Add(new Dictionary<string, object>
        {
            ["Action"] = "Connect",
            ["InterfaceTypeMatch"] = "WiFi",
        });
        // cellular connect:
        rules.Add(new Dictionary<string, object>
        {
            ["Action"] = onDemand.Mode == OnDemandMode.WiFiAndCellular ? "Connect" : "Disconnect",
            ["InterfaceTypeMatch"] = "Cellular",
        });
        // fallback:
        rules.Add(new Dictionary<string, object>
        {
            ["Action"] = onDemand.Mode == OnDemandMode.WiFiAndCellular ? "Connect" : "Disconnect",
        });
        return rules;
    }
}
