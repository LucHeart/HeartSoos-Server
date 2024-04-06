using System.Text.Json;

#pragma warning disable CS8618

namespace LucHeart.HeartSoos.Config;

public static class HeartSoosConfig
{
    private static HeartSoosConf? _internalConfig;
    private static readonly string Path = Directory.GetCurrentDirectory() + "/heartSoosConfig.json";
    private static readonly JsonSerializerOptions SerializerSettings = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static readonly ILogger Logger = ApplicationLogging.CreateLogger(typeof(HeartSoosConfig));

    public static HeartSoosConf Config
    {
        get
        {
            TryLoad();
            return _internalConfig!;
        }
    }

    static HeartSoosConfig()
    {
        TryLoad();
    }

    private static void TryLoad()
    {
        if (_internalConfig != null) return;
        Logger.LogDebug("Loading Config");
        if (File.Exists(Path))
        {
            Logger.LogTrace("Config file exists");
            var json = File.ReadAllText(Path);
            if (!string.IsNullOrWhiteSpace(json))
            {
                Logger.LogTrace("Config file is not empty");
                try
                {
                    _internalConfig = JsonSerializer.Deserialize<HeartSoosConf>(json, SerializerSettings);
                    Logger.LogTrace("Deserialized config");
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error during deserialization");
                }
            }
        }

        if (_internalConfig != null) return;
        Logger.LogDebug("Generating and saving new config file");
        _internalConfig = GetDefaultConfig();
        Save();
    }


    public static void Save()
    {
        Logger.LogDebug("Saving config");
        try
        {
            File.WriteAllText(Path, JsonSerializer.Serialize(_internalConfig, SerializerSettings));
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error occurred while saving");
        }
    }

    private static HeartSoosConf GetDefaultConfig() => new()
    {
        Paths = new[]
        {
            new HeartSoosConf.PushoverConfig
            {
                Name = "default",
                OscConfig = new[]
                {
                    new HeartSoosConf.PushoverConfig.VrChatOscPushover
                    {
                        ParameterPath = "/avatar/parameters/HeartRate"
                    }
                }
            }
        }
    };

    public sealed class HeartSoosConf
    {
        public required IEnumerable<PushoverConfig> Paths { get; set; } = Array.Empty<PushoverConfig>();

        public sealed class PushoverConfig
        {
            public required string Name { get; set; }
            public required IEnumerable<VrChatOscPushover> OscConfig { get; set; } = Array.Empty<VrChatOscPushover>();
            
            public sealed class VrChatOscPushover
            {
                public required string ParameterPath { get; set; }
            }
        }
    }
}