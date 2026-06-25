using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ProductsService _service;

    public ProductsController(ProductsService service)
    {
        _service = service;
    }

    [HttpPost("products")]
    [ProducesResponseType(typeof(WriteResponseDto<ProductResponseDto>), 201)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductDto dto,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return StatusCode(201, result);
    }

    [HttpGet("products")]
    [ProducesResponseType(typeof(PaginatedResult<ProductResponseDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ProductQueryDto query,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("products/{id:guid}")]
    [ProducesResponseType(typeof(ProductResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPatch("products/{id:guid}")]
    [ProducesResponseType(typeof(WriteResponseDto<ProductResponseDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductDto dto,
        CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(result);
    }

    [HttpDelete("products/{id:guid}")]
    [ProducesResponseType(typeof(WriteResponseDto<ProductResponseDto>), 200)]
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
