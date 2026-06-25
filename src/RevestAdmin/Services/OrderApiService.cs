using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using RevestAdmin.Models;

namespace RevestAdmin.Services;

public class OrderApiService(IHttpClientFactory factory) : ApiServiceBase
{
    private HttpClient Client => factory.CreateClient("OrderClient");

    public async Task<PaginatedResult<Order>> GetOrdersAsync(OrderQuery query, CancellationToken ct = default)
    {
        var url = BuildOrderQuery(query);
        var response = await Client.GetAsync(url, ct);
        await ThrowIfError(response);
        return await response.Content.ReadFromJsonAsync<PaginatedResult<Order>>(cancellationToken: ct)
            ?? new PaginatedResult<Order>([], 1, 10, 0, 0, false, false);
    }

    public async Task<Order?> GetOrderAsync(string id, CancellationToken ct = default)
    {
        var response = await Client.GetAsync($"orders/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        await ThrowIfError(response);
        return await response.Content.ReadFromJsonAsync<Order>(cancellationToken: ct);
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest dto, CancellationToken ct = default)
    {
        var response = await Client.PostAsJsonAsync("orders", dto, ct);
        return await ReadWriteResponse<Order>(response);
    }

    public async Task UpdateOrderStatusAsync(string id, string status, CancellationToken ct = default)
    {
        var response = await Client.PatchAsJsonAsync($"orders/{id}", new UpdateOrderStatusRequest(status), ct);
        await ThrowIfError(response);
    }

    public async Task DeleteOrderAsync(string id, CancellationToken ct = default)
    {
        var response = await Client.DeleteAsync($"orders/{id}", ct);
        await ThrowIfError(response);
    }

    private static string BuildOrderQuery(OrderQuery q)
    {
        var p = new Dictionary<string, string?>();
        if (!string.IsNullOrEmpty(q.Search))
        {
            p["search"] = q.Search;
        }

        if (!string.IsNullOrEmpty(q.Status))
        {
            p["status"] = q.Status;
        }

        if (!string.IsNullOrEmpty(q.SortBy))
        {
            p["sortBy"] = q.SortBy;
        }

        if (!string.IsNullOrEmpty(q.SortOrder))
        {
            p["sortOrder"] = q.SortOrder;
        }

        p["page"] = q.Page.ToString();
        p["limit"] = q.Limit.ToString();
        return QueryHelpers.AddQueryString("orders", p!);
    }
}
