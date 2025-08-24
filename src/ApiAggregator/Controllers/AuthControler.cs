using ApiAggregator.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _tokens;
    private readonly AuthOptions _opts;

    public AuthController(IJwtTokenService tokens, Microsoft.Extensions.Options.IOptions<AuthOptions> opts)
    { _tokens = tokens; _opts = opts.Value; }

    [HttpGet("token")]
    [AllowAnonymous]
    public IActionResult Token([FromQuery] string user = "demo")
    {
        if (!_opts.Enabled) return Forbid();
        var jwt = _tokens.IssueToken(user, TimeSpan.FromHours(8));
        return Ok(new { token = jwt });
    }
}