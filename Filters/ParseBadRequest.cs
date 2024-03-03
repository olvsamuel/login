using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace login.Filters
{
    public class ParseBadRequest : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult && objectResult.StatusCode == 400)
            {
                var response = new List<string>();
                if (objectResult.Value is string)
                {
                    response.Add(objectResult.Value.ToString());
                }
                else
                {
                    foreach (var key in context.ModelState.Keys)
                    {
                        foreach (var error in context.ModelState[key].Errors)
                        {
                            response.Add($"{key}: {error.ErrorMessage}");
                        }
                    }
                }

                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Se necessário, adicione lógica para execução antes da ação
        }
    }
}
