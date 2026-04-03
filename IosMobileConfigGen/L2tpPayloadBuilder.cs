using System.Text;

namespace IosMobileConfigGen;

public class L2tpPayloadBuilder : IVpnPayloadBuilder
{
    public Dictionary<string, object> Build(VpnProfileConfig config, string payloadUuid)
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
            ["PPP"] = BuildPpp(config),
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
        return payload;
    }

    private static Dictionary<string, object> BuildPpp(VpnProfileConfig config)
    {
        var ppp = new Dictionary<string, object>
        {
            ["AuthName"] = config.UserName,
            ["CommRemoteAddress"] = config.Server,
        };
        if (!string.IsNullOrEmpty(config.Password))
            ppp["AuthPassword"] = config.Password;
        return ppp;
    }
}
