using ApiAggregator.Models;
using ApiAggregator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AggregateController : ControllerBase
{
    private readonly AggregationService _service;
    private readonly ILogger<AggregateController> _logger;

    public AggregateController(AggregationService service, ILogger<AggregateController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AggregatedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] AggregateQuery query, CancellationToken ct)
    {
        var result = await _service.GetAggregatedAsync(query, ct);
        return Ok(result);
    }
}