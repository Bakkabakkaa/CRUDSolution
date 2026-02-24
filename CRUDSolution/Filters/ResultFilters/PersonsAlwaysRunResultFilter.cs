using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDSolution.Filters.ResultFilters;

public class PersonsAlwaysRunResultFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        throw new NotImplementedException();
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        throw new NotImplementedException();
    }
}