using Microsoft.AspNetCore.Mvc.Filters;
using System;
namespace dotnet_project
{
    public class MyActionFilterAttribute : Attribute, IActionFilter //It is .NET convention to inherit from Atrribute Class if we want Filter on Methods or Controller level Scope
    {
        private readonly string _name;

        public MyActionFilterAttribute(string name) { 
            _name = name;
        }    
        public void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine("OnActionExecuted {0}",_name);
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine("OnActionExecuting {0}", _name);
        }
    }
}
