using Microsoft.AspNetCore.Mvc;

namespace LucHeart.HeartSoos.Controller;

[ApiController]
[Route("/getHr")]
[Route("/hr")]
[Route("/heartrate")]
public class GetHrController : ControllerBase
{
    [HttpGet("{id}")]
    public int Get(string id) => HeartRateManager.GetHeartRate(id) ?? 0;
}