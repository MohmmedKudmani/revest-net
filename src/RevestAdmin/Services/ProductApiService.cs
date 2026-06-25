using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using RevestAdmin.Models;

namespace RevestAdmin.Services;

public class ProductApiService(IHttpClientFactory factory) : ApiServiceBase
{
    private HttpClient Client => factory.CreateClient("ProductClient");

    public async Task<PaginatedResult<Product>> GetProductsAsync(ProductQuery query, CancellationToken ct = default)
    {
        var url = BuildProductQuery(query);
        var response = await Client.GetAsync(url, ct);
        await ThrowIfError(response);
        return await response.Content.ReadFromJsonAsync<PaginatedResult<Product>>(cancellationToken: ct)
            ?? new PaginatedResult<Product>([], 1, 10, 0, 0, false, false);
    }

    public async Task<Product?> GetProductAsync(string id, CancellationToken ct = default)
    {
        var response = await Client.GetAsync($"products/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        await ThrowIfError(response);
        return await response.Content.ReadFromJsonAsync<Product>(cancellationToken: ct);
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest dto, CancellationToken ct = default)
    {
        var response = await Client.PostAsJsonAsync("products", dto, ct);
        return await ReadWriteResponse<Product>(response);
    }

    public async Task<Product> UpdateProductAsync(string id, UpdateProductRequest dto, CancellationToken ct = default)
    {
        var response = await Client.PatchAsJsonAsync($"products/{id}", dto, ct);
        return await ReadWriteResponse<Product>(response);
    }

    public async Task DeleteProductAsync(string id, CancellationToken ct = default)
    {
        var response = await Client.DeleteAsync($"products/{id}", ct);
        await ThrowIfError(response);
    }

    private static string BuildProductQuery(ProductQuery q)
    {
        var p = new Dictionary<string, string?>();
        if (!string.IsNullOrEmpty(q.Search))
        {
            p["search"] = q.Search;
        }

        if (q.MinPrice.HasValue)
        {
            p["minPrice"] = q.MinPrice.ToString();
        }

        if (q.MaxPrice.HasValue)
        {
            p["maxPrice"] = q.MaxPrice.ToString();
        }

        if (q.InStock.HasValue)
        {
            p["inStock"] = q.InStock.ToString()?.ToLower();
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
        return QueryHelpers.AddQueryString("products", p!);
    }
}
