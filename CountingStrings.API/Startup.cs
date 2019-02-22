using System;
using System.Threading;
using CountingStrings.API.Contract;
using CountingStrings.API.Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using Swashbuckle.AspNetCore.Examples;
using Swashbuckle.AspNetCore.Swagger;

namespace CountingStrings.API
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
            #region Bus

            var endpointConfiguration = new EndpointConfiguration("CountingStrings.API");
            endpointConfiguration.SendOnly();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=rabbitmq-countingstringsbroker;username=guest;password=MTk5MTgzNjc4OWNQLjE=");
            transport.UseConventionalRoutingTopology();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(OpenSession).Assembly, "CountingStrings.Service");

            var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            services.AddSingleton<IMessageSession>(endpoint);

            #endregion

            #region Swagger

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "CountingStrings",
                    Description = "Stock Taking App",
                    TermsOfService = "None",
                    Contact = new Contact()
                    {
                        Name = "Cristian Perez Matturro",
                        Email = "crperez.informatica@gmail.com",
                        Url = "https://www.linkedin.com/in/cristianperezmatturro/"
                    }
                });

                c.OperationFilter<ExamplesOperationFilter>();
            });

            #endregion

            services.AddTransient<ICountingStringsRepository, CountingStringsRepository>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CountingStrings v1");
            });

            app.Use(async (ctx, next) =>
            {
                var bus = ctx.RequestServices.GetService<IMessageSession>();
                await bus.Send(new LogRequest
                {
                    Id = Guid.NewGuid(),
                    RequestDate = DateTime.UtcNow
                });

                await next();
            });

            app.UseMvc();
        }
    }
}
