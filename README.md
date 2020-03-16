# rebusspike

Contains the following two .Net Core console applications that run [Rebus](https://github.com/rebus-org/Rebus) with [Topper](https://github.com/rebus-org/Topper) that we used to investigate DB deadlocks in SQL Server.

## [SpikeBus](https://github.com/seankearon/rebusspike/tree/master/SpikeBus)

This project contains a simple [Rebus Bus](https://github.com/seankearon/rebusspike/blob/cd938f2c01f2389451d43edaccdf5f4a13360184/SpikeBus/RebusServiceBus.cs#L51) that uses the SQL Server transport and which runs a very [simple saga](https://github.com/seankearon/rebusspike/blob/cd938f2c01f2389451d43edaccdf5f4a13360184/SpikeBus/RebusServiceBus.cs#L82).

The project starts [4 bus instances](https://github.com/seankearon/rebusspike/blob/cd938f2c01f2389451d43edaccdf5f4a13360184/SpikeBus/Program.cs#L19), 
each with [10 workers and parallelism of 10](https://github.com/seankearon/rebusspike/blob/master/SpikeBus/RebusServiceBus.cs#L57).

## [MessageLoader](https://github.com/seankearon/rebusspike/tree/master/MessageLoader)

This project just messages for the SpikeBus to consume.  

```
cd MessageLoader
dotnet run -- 50
```
