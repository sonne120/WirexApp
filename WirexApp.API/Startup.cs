using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WirexApp.Application;
using WirexApp.Domain;
using WirexApp.Infrastructure;

[assembly: UserSecretsId("54e8eb06-aaa1-4fff-9f05-3ced1cb623c2")]
namespace WirexApp.API
{
    public class Startup
    {

        private readonly IConfiguration _configuration;
        public Startup(IWebHostEnvironment env)
        {
            this._configuration = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .AddJsonFile($"appsettings.{env.EnvironmentName}.json")
                   .AddUserSecrets<Startup>()
                   .Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMemoryCache();
            return services.Initialize(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
