using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDSolution.Filters.ResultFilters;

public class PersonsAlwaysRunResultFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Filters.OfType<SkipFilter>().Any())
        {
            return;
        }
        
        //TO DO: before logic here
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        throw new NotImplementedException();
    }
}