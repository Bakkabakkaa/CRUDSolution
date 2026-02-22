using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDSolution.Filters.ActionFilters;

public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
{
    public int Order { get; set; }
    
    private readonly ILogger<ResponseHeaderActionFilter> _logger;
    private readonly string _key;
    private readonly string _value;

    public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger, string key, string value, int order)
    {
        _logger = logger;
        _key = key;
        _value = value;
        Order = order;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Before
        _logger.LogInformation("{FilterName}.{MethodName} method - before", nameof(ResponseHeaderActionFilter),
            nameof(OnActionExecutionAsync));
        
        await next(); // Calls the subsequent filter or action method
        
        // After
        _logger.LogInformation("{FilterName}.{MethodName} method - after", nameof(ResponseHeaderActionFilter),
            nameof(OnActionExecutionAsync));

        context.HttpContext.Response.Headers[_key] = _value;
    }
}