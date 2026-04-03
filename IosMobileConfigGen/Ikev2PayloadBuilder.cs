using System.Text;

namespace IosMobileConfigGen;

public class Ikev2PayloadBuilder : IVpnPayloadBuilder
{
    public Dictionary<string, object> Build(VpnProfileConfig config, string payloadUuid)
    {
        var payload = new Dictionary<string, object>
        {
            ["IKEv2"] = new Dictionary<string, object>
            {
                ["AuthenticationMethod"] = "SharedSecret",
                ["SharedSecret"] = Encoding.UTF8.GetBytes(config.SharedSecret),
                ["RemoteAddress"] = config.Server,
                ["RemoteIdentifier"] = config.Server,
                ["LocalIdentifier"] = config.UserName,
                // disabled for IKEv2 with PSK only, no EAP:
                // ["AuthName"] = config.UserName,
                // ["AuthPassword"] = config.Password ?? "",
                ["DeadPeerDetectionRate"] = "Medium",
                ["EnablePFS"] = true,
                ["IKESecurityAssociationParameters"] = new Dictionary<string, object>
                {
                    ["EncryptionAlgorithm"] = "AES-256",
                    ["IntegrityAlgorithm"] = "SHA2-256",
                    ["DiffieHellmanGroup"] = 14,
                },
                ["ChildSecurityAssociationParameters"] = new Dictionary<string, object>
                {
                    ["EncryptionAlgorithm"] = "AES-256",
                    ["IntegrityAlgorithm"] = "SHA2-256",
                    ["DiffieHellmanGroup"] = 14,
                },
            },
            ["IPv4"] = new Dictionary<string, object>
            {
                ["OverridePrimary"] = config.SendAllTraffic ? 1 : 0
            },
            ["PayloadDescription"] = $"{config.Organization} IKEv2 VPN profile.",
            ["PayloadDisplayName"] = config.Name,
            ["PayloadIdentifier"] = $"com.apple.vpn.managed.{payloadUuid}",
            ["PayloadOrganization"] = config.Organization,
            ["PayloadType"] = "com.apple.vpn.managed",
            ["PayloadUUID"] = payloadUuid,
            ["PayloadVersion"] = 1,
            ["UserDefinedName"] = config.Name,
            ["VPNType"] = "IKEv2",
        };
        return payload;
    }
}
