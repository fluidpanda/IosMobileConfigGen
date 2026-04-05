using System.Text.Json;

namespace IosMobileConfigGen;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 0;
        }
        if (HasFlag(args, "--init"))
        {
            return GenerateExampleConfig(args);
        }
        try
        {
            var config = BuildConfig(args);
            Validate(config);
            var profile = VpnProfileBuilder.Build(config);
            PlistWriter.Write(profile, config.Output);
            Console.WriteLine($"Profile created: {config.Output}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Error: {exception.Message}");
            return 1;
        }
    }

    private static VpnProfileConfig BuildConfig(string[] args)
    {
        var configPath = GetValue(args, "-c", "--config");
        return LoadJsonConfig(configPath);
    }
    
    private static VpnProfileConfig LoadJsonConfig(string? path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Config file not found: {path}.");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<VpnProfileConfig>(json, AppJsonContext.Default.VpnProfileConfig) ?? 
               throw new InvalidOperationException("Failed to deserialize config. Got null.");
    }
    
    private static bool HasFlag(string[] args, params string[] names)
    {
        return args.Any(arg => names.Contains(arg));
    }

    private static string? GetValue(string[] args, params string[] names)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (names.Contains(args[i]))
                return args[i + 1];
        }
        return null;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("ios-mobile-config-gen --config <path>");
        Console.WriteLine("ios-mobile-config-gen --init");
    }

    private static void Validate(VpnProfileConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Server)) 
            throw new ArgumentException("Server field cannot be empty.");
        if (string.IsNullOrWhiteSpace(config.UserName)) 
            throw new ArgumentException("Username field cannot be empty.");
        if (string.IsNullOrWhiteSpace(config.SharedSecret))
            throw new ArgumentException("Shared secret field cannot be empty.");
    }

    private static int GenerateExampleConfig(string[] args)
    {
        var typeStr = GetValue(args, "--init");
        if (typeStr is null)
        {
            Console.Error.WriteLine("Specify VPN type: --init l2tp, --init ikev2");
            return 1;
        }

        var (config, filename) = typeStr.ToLowerInvariant() switch
        {
            "l2tp" => (Buildl2tpExample(), "vpn-l2tp.json"),
            "ikev2" => (BuildIkev2Example(), "vpn-ikev2.json"),
            _ => throw new ArgumentException($"Unknown VPN type `{typeStr}`, use: l2tp or ikev2")
        };
        var json = JsonSerializer.Serialize(config, AppJsonContext.Default.VpnProfileConfig);
        File.WriteAllText(filename, json + "\n");
        Console.WriteLine($"Config created: {filename}");
        return 0;
    }

    private static VpnProfileConfig Buildl2tpExample() => new()
    {
        VpnType = new VpnTypeConfig
        {
            Type = VpnType.L2TP,
        },
        Name = "L2TP/IPSec VPN Profile",
        Server = "l2tp.domain.com",
        UserName = "username",
        Password = "password",
        SharedSecret = "presharedkey",
        Organization = "Some Organization",
        Identifier = "com.domain.l2tp",
        SendAllTraffic = true,
        OnDemand =  new OnDemandConfig
        {
            Mode = OnDemandMode.WiFiAndCellular,
            ExcludedSsids = ["SSID_1", "SSID_2"],
        }
    };

    private static VpnProfileConfig BuildIkev2Example() => new()
    {
        VpnType = new VpnTypeConfig
        {
            Type = VpnType.IKEv2,
        },
        Name = "IKEv2 VPN Profile",
        Server = "ikev2.domain.com",
        UserName = "username",
        Password = null,
        SharedSecret = "presharedkey",
        Organization = "Some Organization",
        SendAllTraffic = true,
        OnDemand = new OnDemandConfig
        {
            Mode = OnDemandMode.WiFiAndCellular,
            ExcludedSsids = ["SSID_1", "SSID_2"],
        }
    };
}
