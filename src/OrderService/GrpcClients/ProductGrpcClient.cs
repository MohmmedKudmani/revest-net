using Grpc.Core;
using OrderService.Exceptions;
using Revest.Contracts;

namespace OrderService.GrpcClients;

public class ProductGrpcClient
{
    private readonly ProductGrpcService.ProductGrpcServiceClient _client;
    private readonly ILogger<ProductGrpcClient> _logger;

    public ProductGrpcClient(
        ProductGrpcService.ProductGrpcServiceClient client,
        ILogger<ProductGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<ProductMessage?> GetProductAsync(string id, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetProductAsync(
                new GetProductRequest { Id = id },
                cancellationToken: ct);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "gRPC GetProduct failed for id {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<ProductMessage>?> GetAllProductsAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _client.GetAllProductsAsync(
                new GetAllProductsRequest(),
                cancellationToken: ct);
            return result.Products;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "gRPC GetAllProducts failed");
            return null;
        }
    }

    public async Task<IEnumerable<ProductMessage>?> SearchProductsAsync(string name, CancellationToken ct = default)
    {
        try
        {
            var result = await _client.SearchProductsAsync(
                new SearchProductsRequest { Name = name },
                cancellationToken: ct);
            return result.Products;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "gRPC SearchProducts failed for name {Name}", name);
            return null;
        }
    }

    public async Task DecrementStockAsync(string id, int qty, CancellationToken ct = default)
    {
        try
        {
            await _client.DecrementStockAsync(
                new StockOperationRequest { ProductId = id, Quantity = qty },
                cancellationToken: ct);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new NotFoundException("Product not found");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
        {
            throw new BadRequestException(ex.Status.Detail);
        }
    }

    public async Task RestoreStockAsync(string id, int qty, CancellationToken ct = default)
    {
        try
        {
            await _client.RestoreStockAsync(
                new StockOperationRequest { ProductId = id, Quantity = qty },
                cancellationToken: ct);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "gRPC RestoreStock failed for id {Id} qty {Qty} — swallowing", id, qty);
        }
    }
}
