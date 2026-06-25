using System.Text.Json;
using RevestAdmin.Models;

namespace RevestAdmin.Services;

public abstract class ApiServiceBase
{
    // Parse error: message can be string or string[]
    protected static async Task ThrowIfError(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        try
        {
            var json = JsonDocument.Parse(body).RootElement;
            var status = json.GetProperty("status").GetInt32();
            var msgProp = json.GetProperty("message");

            var message = msgProp.ValueKind == JsonValueKind.Array
                ? string.Join(", ", msgProp.EnumerateArray().Select(x => x.GetString()))
                : msgProp.GetString() ?? "Unknown error";

            throw new ApiException(status, message);
        }
        catch (ApiException) { throw; }
        catch { throw new ApiException((int)response.StatusCode, "Unexpected error"); }
    }

    // Read a write response and return the Data field
    protected static async Task<T> ReadWriteResponse<T>(HttpResponseMessage response)
    {
        await ThrowIfError(response);
        var json = await response.Content.ReadFromJsonAsync<WriteResponse<T>>()
            ?? throw new ApiException(500, "Empty response");
        return json.Data;
    }
}
