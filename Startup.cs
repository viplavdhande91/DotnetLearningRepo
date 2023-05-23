using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace dotnet_project
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddTransient<CustomMiddleware1>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            //app.Run(async context =>
            //{
            //    await context.Response.WriteAsync("Hello from Run");
            //});

            //app.Use(async (context, next) =>
            //{
            //    await context.Response.WriteAsync("Hello from Use-1 1 \n");

            //    await next();

            //    await context.Response.WriteAsync("Hello from Use-1 2 \n");
            //});

            app.UseMiddleware<CustomMiddleware1>();

            //app.Map("/nitish", CutomCode);

            //app.Use(async (context, next) =>
            //{
            //    await context.Response.WriteAsync("Hello from Use-3 1 \n");

            //    await next();

            //    await context.Response.WriteAsync("Hello from Use-3 2 \n");
            //});

            //app.Use(async (context, next) =>
            //{
            //    await context.Response.WriteAsync("Request complete \n");
            //});

            //app.Run(async context =>
            //{
            //    await context.Response.WriteAsync("Hello from Run \n");
            //});

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
