using System.Text.Json;
using IosMobileConfigGen;

var config = new VpnProfileConfig
{
    Name = "Test VPN",
    Server = "vpn.example.com",
    UserName =  "testuser",
    Password = "testpass",
    SharedSecret = "testsecret",
    Organization = "testorg",
    Identifier = "net.testidentifier.vpn",
    OnDemand = new OnDemandConfig
    {
        Mode = OnDemandMode.WiFiAndCellular,
        ExcludedSsids = ["SomeWifi", "WorkWifi"]
    }
};

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
};

var json = JsonSerializer.Serialize(config, jsonOptions);
Console.WriteLine("json config");
Console.WriteLine(json);
Console.WriteLine();

var profile = VpnProfileBuilder.Build(config);
PlistWriter.Write(profile, config.Output);

Console.WriteLine($"profile output: {config.Output}");
Console.WriteLine(File.ReadAllText(config.Output));