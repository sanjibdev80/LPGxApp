using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RdlcWebApi.Class;
using RdlcWebApi.Services;
using System;

namespace RdlcWebApi
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
            // Add controllers services
            services.AddControllers();

            // Register your custom services
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ApiService>();  // Register ApiService

            // Add HttpClient for ApiService (if it requires HTTP calls)
            services.AddHttpClient<ApiService>(client =>
            {
                client.BaseAddress = new Uri(Configuration.GetValue<string>("ApiSettings:BaseUrl"));
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // Add Swagger services for API documentation
            services.AddSwaggerGen();

            // Optionally configure other services, like caching, authentication, etc., here
            services.AddLogging(builder => builder.AddConsole());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RdlcWebApi v1");
                    c.RoutePrefix = "swagger";  // Swagger UI at /swagger
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RdlcWebApi v1");
                    c.RoutePrefix = string.Empty;  // Swagger UI at root
                });
            }

            // Enable middleware to serve static files (including Swagger UI)
            app.UseStaticFiles();

            // Always redirect HTTP requests to HTTPS
            app.UseHttpsRedirection();

            // Enable routing
            app.UseRouting();

            // Add authorization middleware
            app.UseAuthorization();

            // Map controllers to endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
