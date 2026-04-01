using System.Text;

namespace IosMobileConfigGen;

/// <summary>
/// Builds an Apple Configuration Profile dictionary tree for l2tp/ipsec VPN.
/// </summary>
public class VpnProfileBuilder
{
    public Dictionary<string, object> Build(VpnProfileConfig config)
    {
        string vpnPayloadUuid = Guid.NewGuid().ToString().ToUpperInvariant();
        Dictionary<string, object> vpnPayload = BuildVpnPayload(config, vpnPayloadUuid);

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

    public Dictionary<string, object> BuildVpnPayload(VpnProfileConfig config, string payloadUuid)
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

    public Dictionary<string, object> BuildPppPayload(VpnProfileConfig config)
    {
        var ppp = new Dictionary<string, object>
        {
            ["AuthName"] = config.UserName,
            ["CommRemoteAddress"] = config.Server,
        };
        
        // password is optional - if omitted, the device prompts on each connect
        if (!string.IsNullOrEmpty(config.Password))
            ppp["AuthPassword"] = config.Password;

        return ppp;
    }

    public List<object> BuildOnDemandRules(OnDemandConfig onDemand)
    {
        var rules = new List<object>();
        
        // disconnect when connected to an excluded SSID
        if (onDemand.ExcludedSsids.Length > 0)
        {
            rules.Add(new Dictionary<string, object>
            {
                ["Action"] = "Disconnect",
                ["InterfaceTypeMatch"] = "WiFi",
                ["SSIDMatch"] = new List<object>(onDemand.ExcludedSsids.Cast<object>()),
            });
        }
        
        // connect on any other wifi networks
        rules.Add(new Dictionary<string, object>
        {
            ["Action"] = "Connect",
            ["InterfaceTypeMatch"] = "WiFi",
        });
        
        // cellular connect
        rules.Add(new Dictionary<string, object>
        {
            ["Action"] = onDemand.Mode == OnDemandMode.WiFiAndCellular ? "Connect" : "Disconnect",
            ["InterfaceTypeMatch"] = "Cellular",
        });
        
        // fallback
        rules.Add(new Dictionary<string, object>
        {
            ["Action"] = onDemand.Mode == OnDemandMode.WiFiAndCellular ? "Connect" : "Disconnect",
        });
        
        return rules;
    }
}
