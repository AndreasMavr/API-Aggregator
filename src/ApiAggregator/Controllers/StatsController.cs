using ApiAggregator.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly IStatsStore _store;
    public StatsController(IStatsStore store) => _store = store;

    [HttpGet]
    public IActionResult Get() => Ok(_store.ToSnapshot());
}