using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDSolution.Filters.ActionFilters;

public class PersonsListActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        throw new NotImplementedException();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        throw new NotImplementedException();
    }
}