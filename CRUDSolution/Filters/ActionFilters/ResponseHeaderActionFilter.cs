using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDSolution.Filters.ActionFilters;

public class ResponseHeaderActionFilter : IActionFilter, IOrderedFilter
{
    public int Order { get; set; }
    
    private readonly ILogger<ResponseHeaderActionFilter> _logger;
    private readonly string Key;
    private readonly string Value;

    public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger, string key, string value, int order)
    {
        _logger = logger;
        Key = key;
        Value = value;
        Order = order;
    }
    
    // Before
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("{FilterName}.{MethodName} method", nameof(ResponseHeaderActionFilter),
            nameof(OnActionExecuting));
    }

    // After
    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("{FilterName}.{MethodName} method", nameof(ResponseHeaderActionFilter),
            nameof(OnActionExecuted));

        context.HttpContext.Response.Headers[Key] = Value;
    }
}