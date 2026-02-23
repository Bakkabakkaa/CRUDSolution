using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDSolution.Filters.ResourceFilters;

public class FeatureDisabledResourceFilter : IAsyncResourceFilter
{
    public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        throw new NotImplementedException();
    }
}