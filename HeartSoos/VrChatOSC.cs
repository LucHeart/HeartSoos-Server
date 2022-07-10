using System.Net;
using Rug.Osc;

namespace LucHeart.HeartSoos;

public class VrChatOSC : IDisposable
{

    private ILogger _logger = ApplicationLogging.CreateLogger<VrChatOSC>();

    private string _id;
    private string _parameter;
    private int _lastHeartRate = -1;
    private DateTime _lastUpdate = DateTime.UnixEpoch;
    private OscSender _sender;

    private CancellationTokenSource _cts;
    
    public VrChatOSC(string id, string parameter = "/avatar/parameters/HeartRate")
    {
        _cts = new CancellationTokenSource();
        _id = id;
        _parameter = parameter;
        
        _sender = new OscSender(IPAddress.Loopback, 0, 9000);
        _sender.Connect();
        Task.Run(Check);
    }

    private void SendUpdate(int heartRate)
    {
        _lastHeartRate = heartRate;
        var fl = ConvertToVrcFloat(heartRate);
        _sender.Send(new OscMessage(_parameter, fl));
        _logger.LogInformation("Sending update to VRC for {Id} with heart rate {Hr}", _id, heartRate);
    }

    private static float ConvertToVrcFloat(int heartRate) => heartRate / 255f * 2f - 1f;

    private async Task Check()
    {
        while (!_cts.IsCancellationRequested)
        {
            var heartRate = GetHeartRate();
            if(!heartRate.HasValue) continue;
            if(_lastHeartRate != heartRate && _lastUpdate.AddSeconds(1) < DateTime.UtcNow)
            {
                _lastUpdate = DateTime.UtcNow;
                SendUpdate(heartRate.Value);
            }
            await Task.Delay(200, _cts.Token);
        }
    }

    private int? GetHeartRate() => HeartRateManager.GetHeartRate(_id)!;

    public void Dispose()
    {
        _cts.Cancel();
        _sender.Dispose();
        _cts.Dispose();
    }
}