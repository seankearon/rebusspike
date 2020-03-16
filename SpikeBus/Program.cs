using System;
using Serilog;
using Topper;

namespace SpikeBus
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.RollingFile("spike-bus-{Date}.log")
                .WriteTo.ColoredConsole().CreateLogger();

            Console.WriteLine("Hello World!");
            ServiceHost.Run(
                new ServiceConfiguration()
                    .Add("MyLovelyBus_1", () => new RebusServiceBus())
                    .Add("MyLovelyBus_2", () => new RebusServiceBus())
                    .Add("MyLovelyBus_3", () => new RebusServiceBus())
                    .Add("MyLovelyBus_4", () => new RebusServiceBus()));
        }
    }
}
