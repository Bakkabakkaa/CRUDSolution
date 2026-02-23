using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDSolution.Filters.ExceptionFilters;

public class HandleExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        throw new NotImplementedException();
    }
}