using Grpc.Core;
using ProductService.Exceptions;
using ProductService.Services;
using Revest.Contracts;

namespace ProductService.GrpcServices;

public class ProductGrpcService : global::Revest.Contracts.ProductGrpcService.ProductGrpcServiceBase
{
    private readonly ProductsService _service;

    public ProductGrpcService(ProductsService service)
    {
        _service = service;
    }

    public override async Task<ProductMessage> GetProduct(
        GetProductRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var id))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid product id format"));
            }

            var dto = await _service.GetByIdAsync(id, context.CancellationToken);
            return ToMessage(dto);
        }
        catch (NotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<ProductListMessage> GetAllProducts(
        GetAllProductsRequest request,
        ServerCallContext context)
    {
        var products = await _service.GetAllRawAsync(context.CancellationToken);
        var reply = new ProductListMessage();
        reply.Products.AddRange(products.Select(ToMessage));
        return reply;
    }

    public override async Task<ProductListMessage> SearchProducts(
        SearchProductsRequest request,
        ServerCallContext context)
    {
        var products = await _service.SearchByNameAsync(request.Name, context.CancellationToken);
        var reply = new ProductListMessage();
        reply.Products.AddRange(products.Select(ToMessage));
        return reply;
    }

    public override async Task<StockOperationResult> DecrementStock(
        StockOperationRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ProductId, out var id))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid product id format"));
            }

            await _service.DecrementStockAsync(id, request.Quantity, context.CancellationToken);
            return new StockOperationResult { Success = true, Message = "Stock decremented" };
        }
        catch (NotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (BadRequestException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<StockOperationResult> RestoreStock(
        StockOperationRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ProductId, out var id))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid product id format"));
            }

            await _service.RestoreStockAsync(id, request.Quantity, context.CancellationToken);
            return new StockOperationResult { Success = true, Message = "Stock restored" };
        }
        catch (NotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    private static ProductMessage ToMessage(DTOs.ProductResponseDto dto) =>
        new()
        {
            Id = dto.Id.ToString(),
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            Price = (double)dto.Price,
            Stock = dto.Stock,
            CreatedAt = dto.CreatedAt.ToString("O"),
            UpdatedAt = dto.UpdatedAt.ToString("O"),
        };
}
