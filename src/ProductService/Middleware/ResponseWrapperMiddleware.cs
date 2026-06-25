using System.Text;
using System.Text.Json;

namespace ProductService.Middleware;

public class ResponseWrapperMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public ResponseWrapperMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method.ToUpper();
        if (method is not ("POST" or "PATCH" or "DELETE"))
        {
            await _next(context);
            return;
        }

        // Skip gRPC requests — their binary protobuf bodies must not be wrapped
        var requestContentType = context.Request.ContentType ?? string.Empty;
        if (requestContentType.StartsWith("application/grpc", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await _next(context);

        var statusCode = context.Response.StatusCode;

        // Only wrap successful mutation responses
        if (statusCode < 200 || statusCode >= 300)
        {
            buffer.Seek(0, SeekOrigin.Begin);
            await buffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
            return;
        }

        buffer.Seek(0, SeekOrigin.Begin);
        var bodyText = await new StreamReader(buffer).ReadToEndAsync();

        string action = method switch
        {
            "POST" => "created",
            "PATCH" => "updated",
            "DELETE" => "deleted",
            _ => "processed",
        };

        // Try to extract the "name" field from the existing response payload
        string productName = string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(bodyText);
            if (doc.RootElement.TryGetProperty("name", out var nameProp))
            {
                productName = nameProp.GetString() ?? string.Empty;
            }
        }
        catch
        {
            // ignore parse errors — wrap anyway without name
        }

        var message = string.IsNullOrEmpty(productName)
            ? $"Product {action} successfully"
            : $"Product \"{productName}\" {action} successfully";

        JsonElement? dataElement = null;
        try
        {
            using var doc = JsonDocument.Parse(bodyText);
            dataElement = doc.RootElement.Clone();
        }
        catch
        {
            // fall through — send as-is
        }

        var wrapped = new
        {
            message,
            data = dataElement,
        };

        var wrappedJson = JsonSerializer.Serialize(wrapped, _jsonOptions);
        var wrappedBytes = Encoding.UTF8.GetBytes(wrappedJson);

        context.Response.Body = originalBody;
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.ContentLength = wrappedBytes.Length;
        await context.Response.Body.WriteAsync(wrappedBytes);
    }
}
