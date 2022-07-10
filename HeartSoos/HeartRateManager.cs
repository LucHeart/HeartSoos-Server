namespace LucHeart.HeartSoos;

public static class HeartRateManager
{
    private static readonly Dictionary<string, int> HeartRates = new();
    private static List<VrChatOSC> _vrChatOSCs = new();

    public static void SetHeartRate(string id, int heartRate) => HeartRates[id] = heartRate;

    public static int? GetHeartRate(string id) => HeartRates.ContainsKey(id) ? HeartRates[id] : null;

    public static void AddVrChatOSC(string id, string parameter) => _vrChatOSCs.Add(new VrChatOSC(id, parameter));
}