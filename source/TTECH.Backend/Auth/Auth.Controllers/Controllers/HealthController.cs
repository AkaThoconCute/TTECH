using Microsoft.AspNetCore.Mvc;

namespace Auth.AppHost.Controllers
{
  [ApiController]
  public class HealthController : ControllerBase
  {
    [HttpGet("/")]
    [HttpGet("/health")]
    public IActionResult GetHealthStatus()
    {
      return Ok(new
      {
        status = "Healthy",
        timestamp = DateTime.UtcNow
      });
    }
  }
}
