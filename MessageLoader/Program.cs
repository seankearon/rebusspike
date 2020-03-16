using System;
using System.IO;
using System.Linq;
using Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;

namespace MessageLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();
            var services = new ServiceCollection();
            services.AddRebus(x => ConfigureRebus(x, config));

            var provider = services.BuildServiceProvider();
            provider.UseRebus(bus =>
            {
                var messageCount = MessageCount(args);
                Console.WriteLine($"Sending {messageCount} messages...");
                foreach (var _ in Enumerable.Range(1, messageCount))
                {
                    bus.Advanced.Routing.Send("ServiceBus", new StartSaga {Name = Path.GetTempFileName()});
                }
                Console.WriteLine("Done.");
            });
        }

        static int MessageCount(string[] args) => args != null && args.Length > 0 && int.TryParse(args[0], out var i) ? i : 200;

        private static RebusConfigurer ConfigureRebus(RebusConfigurer c, IConfigurationRoot config)
        {
            var connectionString = config.GetValue<string>("ConnectionString");
            //var sqlOptions = new SqlServerTransportOptions(connectionString);

            return c.Logging(x => x.ColoredConsole())
                .Routing(x => x.TypeBased().Map<StartSaga>("ServiceBus"))
                .Options(x => x.SimpleRetryStrategy("Error"))
                .Transport(x => x
                        .UseSqlServerAsOneWayClient(connectionString)
                    )
                .Subscriptions(x => x.StoreInSqlServer(connectionString, "Subscriptions", isCentralized: true, automaticallyCreateTables: false));
        }
    }
}
