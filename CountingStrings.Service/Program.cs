using System;
using NServiceBus;

namespace CountingStrings.Service
{
    static class Program
    {
        static void Main(string[] args)
        {
            var endpointConfiguration = new EndpointConfiguration("CountingStrings.Service");
            #region TransportConfiguration

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=rabbitmq");
            transport.UseConventionalRoutingTopology();

            #endregion

            endpointConfiguration.EnableInstallers();

            var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

            Console.Read();
            endpoint.Stop().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
