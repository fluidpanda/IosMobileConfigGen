using System.Text.Json;

namespace IosMobileConfigGen;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = AppJsonContext.Default.Options;

    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 0;
        }
        if (HasFlag(args, "--init"))
        {
            return GenerateExampleConfig();
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

    private static int GenerateExampleConfig()
    {
        const string path = "vpn-config.json";
        var example = new VpnProfileConfig
        {
            Name = "VPN Profile",
            Server = "vpn.example.com",
            UserName = "username",
            Password = "password",
            SharedSecret = "presharedkey",
            Organization = "Some Organization",
            Identifier = "com.domain.vpn",
            SendAllTraffic = true,
            OnDemand = new OnDemandConfig
            {
                Mode = OnDemandMode.WiFiAndCellular,
                ExcludedSsids = ["WiFiSSID_1", "WiFiSSID_2"]
            },
            Output = "vpn.mobileconfig"
        };
        var json = JsonSerializer.Serialize(example, AppJsonContext.Default.VpnProfileConfig);
        File.WriteAllText(path, json + "\n");
        return 0;
    }
}
