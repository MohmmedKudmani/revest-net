using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
public class OrdersController : ControllerBase
{
    private readonly OrdersService _service;

    public OrdersController(OrdersService service)
    {
        _service = service;
    }

    [HttpPost("orders")]
    [ProducesResponseType(typeof(WriteResponseDto<OrderResponseDto>), 201)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return StatusCode(201, result);
    }

    [HttpGet("orders")]
    [ProducesResponseType(typeof(PaginatedResult<OrderResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] OrderQueryDto query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("orders/{id:guid}")]
    [ProducesResponseType(typeof(OrderResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPatch("orders/{id:guid}")]
    [ProducesResponseType(typeof(WriteResponseDto<OrderResponseDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(result);
    }

    [HttpDelete("orders/{id:guid}")]
    [ProducesResponseType(typeof(WriteResponseDto<OrderResponseDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("health")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Health() => Ok(new { status = "ok" });
}
