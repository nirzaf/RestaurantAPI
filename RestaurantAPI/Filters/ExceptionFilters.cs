using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RestaurantAPI.Filters;

public class ExceptionFilters : IExceptionFilter
{
    private readonly ILogger<ExceptionFilters> _logger;

    public ExceptionFilters(ILogger<ExceptionFilters> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, context.Exception.Message);
        
        var exceptionType = context.Exception.GetType();
        if (exceptionType == typeof(ValidationException))
        {
            context.Result = new BadRequestObjectResult(new { message = context.Exception.Message });
            return;
        }
        
        var result = new JsonResult("Something went wrong")
        {
            StatusCode = 500
        };
        context.Result = result;
    }
}