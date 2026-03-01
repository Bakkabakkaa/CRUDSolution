using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDSolution.Filters.ActionFilters;

public class ResponseHeaderFilteredFactoryAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;
    public string? Key { get; set; }
    public string? Value { get; set; }
    public int Order { get; set; }

    public ResponseHeaderFilteredFactoryAttribute(string key, string value, int order)
    {
        Key = key;
        Value = value;
        Order = order;
    }
    
    // Controler -> FilterFactory -> Filter
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var filter = serviceProvider.GetRequiredService<ResponseHeaderActionFilter>();
        filter.Key = Key;
        filter.Value = Value;
        filter.Order = Order;
        // Return filter object
        return filter;
    }
}

public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
{
    public string Key { get; set; }
    public string Value { get; set; }
    public int Order { get; set; }

    private readonly ILogger<ResponseHeaderActionFilter> _logger;

    public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger)
    {
        _logger = logger;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Before
        _logger.LogInformation("Before logic - ResponseHeaderActionFilter");
        await next(); // Calls the subsequent filter or action method
        
        // After
        _logger.LogInformation("After logic - ResponseHeaderActionFilter");

        context.HttpContext.Response.Headers[Key] = Value;
    }
}