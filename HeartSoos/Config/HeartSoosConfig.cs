using Newtonsoft.Json;

namespace LucHeart.HeartSoos.Config;

public static class HeartSoosConfig
{
    private static HeartSoosConf? _internalConfig;
    private static readonly string Path = Directory.GetCurrentDirectory() + "/heartSoosConfig.json";
    private static readonly JsonSerializerSettings SerializerSettings = new();
    private static ILogger _logger = ApplicationLogging.CreateLogger(typeof(HeartSoosConfig));

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
        _logger.LogDebug("Loading Config");
        if (File.Exists(Path))
        {
            _logger.LogTrace("Config file exists");
            var json = File.ReadAllText(Path);
            if (!string.IsNullOrWhiteSpace(json))
            {
                _logger.LogTrace("Config file is not empty");
                try
                {
                    _internalConfig = JsonConvert.DeserializeObject<HeartSoosConf>(json, SerializerSettings);
                    _logger.LogTrace("Deserialized config");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error during deserialization");
                }
            }
        }

        if (_internalConfig != null) return;
        _logger.LogDebug("Generating and saving new config file");
        _internalConfig = GetDefaultConfig();
        Save();
    }


    public static async Task Save()
    {
        _logger.LogDebug("Saving config");
        try
        {
            await File.WriteAllTextAsync(Path, JsonConvert.SerializeObject(_internalConfig));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while saving");
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

    public class HeartSoosConf
    {
        public IEnumerable<PushoverConfig> Paths { get; set; } = Array.Empty<PushoverConfig>();

        public class PushoverConfig
        {
            public string Name { get; set; }
            public IEnumerable<VrChatOscPushover> OscConfig { get; set; } = Array.Empty<VrChatOscPushover>();
            public class VrChatOscPushover
            {
                public string ParameterPath { get; set; }
            }
        }
    }
}