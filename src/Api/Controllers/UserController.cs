using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetUser")]
    public IActionResult Get()
    {
        var claims = User.Claims.Select(claims => new { claims.Type, claims.Value });
        return Ok(new { Message = "Authenticated via GitHub", Claims = claims });
    }
}
