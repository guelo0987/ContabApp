using Microsoft.AspNetCore.Mvc;

namespace ContabBackApp.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Ping()
    {
        return Ok(new 
        { 
            status = "online", 
            serverTime = DateTime.Now, 
            message = "Backend funcionando correctamente 🚀" 
        });
    }
}
