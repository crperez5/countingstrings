using System;
using System.IO;
using AutoMapper;
using CountingStrings.Service.Data;
using CountingStrings.Service.Data.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace CountingStrings.Worker
{
    static class Program
    {
        static void Main(string[] args)
        {
            var configuration = GetServiceConfiguration();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);

            services.AddDbContext<CountingStringsContext>(o =>
                o.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddAutoMapper(x => x.AddProfile(new SessionMapping()));

            var endpoint = GetConfiguredEndpoint(services);

            endpoint.ScheduleEvery(TimeSpan.FromSeconds(10),
                    pipelineContext => pipelineContext.SendLocal(new RefreshCalculations())).ConfigureAwait(false)
                .GetAwaiter().GetResult();

            Console.Read();
            endpoint.Stop().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static IConfigurationRoot GetServiceConfiguration()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();
            return configuration;
        }

        private static IEndpointInstance GetConfiguredEndpoint(IServiceCollection services)
        {
            var endpointConfiguration = new EndpointConfiguration("CountingStrings.Worker");

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=rabbitmq");
            transport.UseConventionalRoutingTopology();

            endpointConfiguration.EnableInstallers();

            endpointConfiguration.UseContainer<ServicesBuilder>(
                customizations: customizations =>
                {
                    customizations.ExistingServices(services);
                });

            var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            return endpoint;
        }
    }
}
