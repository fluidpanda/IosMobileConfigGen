using System.Text;

namespace IosMobileConfigGen;

/// <summary>
/// Builds an Apple Configuration Profile dictionary tree for L2TP/IPSec VPN.
/// </summary>
public static class VpnProfileBuilder
{
    public static Dictionary<string, object> Build(VpnProfileConfig config)
    {
        var vpnPayloadUuid = Guid.NewGuid().ToString().ToUpperInvariant();
        var vpnPayload = BuildVpnPayload(config, vpnPayloadUuid);
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

    private static Dictionary<string, object> BuildVpnPayload(VpnProfileConfig config, string payloadUuid)
    {
        var payload = new Dictionary<string, object>
        {
            ["IPSec"] = new Dictionary<string, object>
            {
                ["AuthenticationMethod"] = "SharedSecret",
                ["LocalIdentifierType"] = "KeyID",
                ["SharedSecret"] = Encoding.UTF8.GetBytes(config.SharedSecret),
            },
            ["IPv4"] = new Dictionary<string, object>
            {
                ["OverridePrimary"] = config.SendAllTraffic ? 1 : 0
            },
            ["PPP"] = BuildPppPayload(config),
            ["PayloadDescription"] = $"{config.Organization} L2TP/IPSec VPN profile.",
            ["PayloadDisplayName"] = config.Name,
            ["PayloadIdentifier"] = $"com.apple.vpn.managed.{payloadUuid}",
            ["PayloadOrganization"] = config.Organization,
            ["PayloadType"] = "com.apple.vpn.managed",
            ["PayloadUUID"] = payloadUuid,
            ["PayloadVersion"] = 1,
            ["UserDefinedName"] = config.Name,
            ["VPNType"] = "L2TP",
        };
        if (config.OnDemand is { Mode: not OnDemandMode.Disabled } onDemand)
        {
            payload["OnDemandEnabled"] = 1;
            payload["OnDemandRules"] = BuildOnDemandRules(onDemand);
        }
        return payload;
    }

    private static Dictionary<string, object> BuildPppPayload(VpnProfileConfig config)
    {
        var ppp = new Dictionary<string, object>
        {
            ["AuthName"] = config.UserName,
            ["CommRemoteAddress"] = config.Server,
        };
        // password is optional - if omitted, the device prompts on each connect:
        if (!string.IsNullOrEmpty(config.Password))
            ppp["AuthPassword"] = config.Password;
        return ppp;
    }

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
