using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrderService.Exceptions;

namespace OrderService.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var (status, message) = context.Exception switch
        {
            NotFoundException ex => (404, (object)ex.Message),
            BadRequestException ex => (400, (object)ex.Message),
            ValidationException ex => (400, (object)ex.Errors.Select(e => e.ErrorMessage).ToArray()),
            _ => (500, (object)"Something went wrong. Please try again later."),
        };

        if (status == 500)
        {
            _logger.LogError(context.Exception, "Unhandled exception");
        }

        context.Result = new ObjectResult(new { status, message })
        {
            StatusCode = status,
        };

        context.ExceptionHandled = true;
    }
}
