using CountingStrings.API.Contract;
using CountingStrings.API.DataAccess.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
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

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=rabbitmq");
            transport.UseConventionalRoutingTopology();

            endpointConfiguration.SendOnly();

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
            });

            #endregion

            services.AddTransient<IInventoryRepository, InventoryRepository>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            app.UseMvc();
        }
    }
}
