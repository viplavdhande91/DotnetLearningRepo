﻿using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace dotnet_project
{
  


      public class CustomMiddleware1 : IMiddleware
        {
            public async Task InvokeAsync(HttpContext context, RequestDelegate next)
            {
                await context.Response.WriteAsync("Hello from Custom middleware file 1 \n");

                await next(context);

                await context.Response.WriteAsync("Hello from Custom middleware file 2 \n");
        }
    }
    
}
