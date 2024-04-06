using LucHeart.HeartSoos.Models;
using Microsoft.AspNetCore.Mvc;

namespace LucHeart.HeartSoos.Controller;

[ApiController]
[Route("/heartsoos")]
public sealed class HeartSoosController : ControllerBase
{
    [HttpPost("{id}")]
    public IActionResult Get(string id, HeartSoosWsData data)
    {
        HeartRateManager.SetHeartRate(id, data.HeartRate);
        return Ok();
    }
}