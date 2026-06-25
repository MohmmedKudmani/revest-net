namespace RevestAdmin.Models;

// Write response wrapper: POST/PATCH/DELETE return this
public record WriteResponse<T>(string Message, T Data);

// Error shape from both services
public record ApiErrorResponse(int Status, object Message); // message can be string or string[]

public class ApiException(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
}
