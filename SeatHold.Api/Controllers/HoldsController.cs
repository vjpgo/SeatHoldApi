namespace SeatHold.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using SeatHold.Core.Contracts;
using SeatHold.Core.Services;

[ApiController]
[Route("holds")]
public sealed class HoldsController : ControllerBase
{
    private readonly IHoldService _service;

    public HoldsController(IHoldService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(HoldResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateHold([FromBody] CreateHoldRequest request, CancellationToken ct)
    {
        var created = await _service.CreateHoldAsync(request, ct).ConfigureAwait(false);

        return CreatedAtAction(
            actionName: nameof(GetHold),
            routeValues: new { id = created.Id },
            value: created);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HoldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHold([FromRoute] Guid id, CancellationToken ct)
    {
        var hold = await _service.GetHoldAsync(id, ct).ConfigureAwait(false);
        if (hold is null)
        {
            return NotFound();
        }

        return Ok(hold);
    }

    // Diagnostic endpoint -> Get all holds
    // GET /holds?status=active|expired
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HoldResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHolds([FromQuery] string? status, CancellationToken ct)
    {
        HoldStatusFilter? filter = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
            {
                filter = HoldStatusFilter.Active;
            }
            else if (string.Equals(status, "expired", StringComparison.OrdinalIgnoreCase))
            {
                filter = HoldStatusFilter.Expired;
            }
            else
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid query parameter",
                    Detail = "status must be 'active' or 'expired'.",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        var holds = await _service.GetHoldsAsync(filter, ct).ConfigureAwait(false);
        return Ok(holds);
    }
}
