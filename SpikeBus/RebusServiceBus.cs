using System;
using System.Threading.Tasks;
using Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Auditing.Messages;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Retry.Simple;
using Rebus.Sagas;
using Rebus.ServiceProvider;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SpikeBus
{
    public class RebusServiceBus : IDisposable
    {
        public RebusServiceBus()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();
            var services = new ServiceCollection();
            services.AddLogging(l => l.AddSerilog());
            services.AutoRegisterHandlersFromAssemblyOf<Program>();
            services.AddRebus(x => ConfigureRebus(x, config));

            Provider = services.BuildServiceProvider();
            Provider.UseRebus(bus =>
            {
                Subscribe(bus);
                Bus = bus;
            });
        }

        private IBus Bus { get; set; }
        private ServiceProvider Provider { get; }

        public void Dispose()
        {
            Bus?.Dispose();
            Provider?.Dispose();
        }

        private RebusConfigurer ConfigureRebus(RebusConfigurer c, IConfigurationRoot config)
        {
            var connectionString = config.GetValue<string>("ConnectionString");
            //var sqlOptions = new SqlServerTransportOptions(connectionString);

            return c
                .Logging(x => { x.Serilog(); })
                .Options(x =>
                {
                    x.SimpleRetryStrategy("Error");
                    x.EnableMessageAuditing("Audit");
                    x.SetNumberOfWorkers(10);
                    x.SetMaxParallelism(10);
                })
                .Transport(x => x
                        .UseSqlServer(connectionString, "ServiceBus")
                )
                .Subscriptions(x => x.StoreInSqlServer(connectionString, "Subscriptions", isCentralized: true, automaticallyCreateTables: false))
                .Sagas(x => x.StoreInSqlServer(connectionString, "Sagas", "SagaIndexes"));
        }

        private void Subscribe(IBus bus)
        {
            bus.Subscribe<StartSaga>();
        }
    }


    public class StupidSagaData: ISagaData
    {
            public Guid Id { get; set; }
            public int Revision { get; set; }

            public string Name { get; set; }
    }

    public class StupidSaga : Saga<StupidSagaData>, 
        IAmInitiatedBy<StartSaga>,
        IHandleMessages<EndSaga>
    {
        private readonly IBus _bus;
        private readonly ILogger _log;

        public StupidSaga(IBus bus, ILogger<StupidSaga> log)
        {
            _bus = bus;
            _log = log;
        }

        protected override void CorrelateMessages(ICorrelationConfig<StupidSagaData> config)
        {
            config.Correlate<StartSaga>(x => x.Name, x => x.Name);
            config.Correlate<EndSaga>(x => x.Name, x => x.Name);
        }

        public Task Handle(StartSaga message)
        {
            if (!IsNew) return Task.CompletedTask;
            _log.LogInformation($"Thanks, {message.Name}, that was a lovely message!");
            return _bus.SendLocal(new EndSaga {Name = message.Name});
        }

        public Task Handle(EndSaga message)
        {
            MarkAsComplete();
            return Task.CompletedTask;
        }
    }
}