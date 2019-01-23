using CountingStrings.API.Bus;
using CountingStrings.API.DataAccess.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

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
            #region EndpointConfiguration

            var endpointConfiguration = new EndpointConfiguration("CountingStrings.API");
            #region TransportConfiguration
            
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=rabbitmq");
            transport.UseConventionalRoutingTopology();

            #endregion

            endpointConfiguration.SendOnly();

            #endregion

            #region Routing

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(MyMessage).Assembly, "CountingStrings.Service");

            #endregion

            #region EndpointStart

            var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

            #endregion

            #region ServiceRegistration

            services.AddSingleton<IMessageSession>(endpoint);

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

            app.UseMvc();
        }
    }
}
