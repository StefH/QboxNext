# QboxNext.ConsoleApp.Core

This library contains core classes that help with creating command line utilities.

## ConsoleHostBuilder

The `ConsoleHostBuilder` extends [Microsoft.Extensions.Hosping.HostBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.hostbuilder?view=aspnetcore-2.2) and returns a host that is preconfigured for QboxNext console utilities or services.

### Command line arguments

An extension is provided that uses the [Command Line Parser library](https://github.com/commandlineparser/commandline) to parse command line arguments.

To parse command line arguments into an options class, call `ParseArguments` with the type of the options class, and a parser instance.

#### Example

```csharp
IHost host = new ConsoleHostBuilder(args)
    .ParseArguments<MyCommandLineOptions>(Parser.Default)
    .ConfigureServices((context, services) => {
        services.AddHostedService<ConsoleRunner>();
    })
    .Build();
```

The options are parsed and injected into the service container so that you can request it via dependency injection:

```csharp
public class ConsoleRunner : IHostedService
{
    public ConsoleRunner(IOptions<MyCommandLineOptions> options) {
        // Use the options...
    }
}
```

### Running the host

There are two ways of running a host.

- `StartWithArgumentsAsync` is similar to `StartAsync`
- `RunWithArgumentsAsync` is similar to `RunAsync`

In either case, the command line options are checked if they are succesfully parsed. If not, the usage/help screen is shown. It will also handle any unhandled exceptions.

### Handling CTRL+C / SIGTERM

To allow the app to terminate prematurely via CTRL+C / SIGTERM, inject an instance of `IApplicationLifetime`. Then simply periodically check the `ApplicationStopping` cancellation token, to stop any long running loops.

```csharp
public class ConsoleRunner : IHostedService
{
    public ConsoleRunner(IApplicationLifetime applicationLifetime) {
        _applicationLifetime = applicationLifetime
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        // Doing intensive processing/work.
        for (int i=0; i < 10000000; i++)
        {
            // We can check for CTRL+C to abort our work.
            if (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                break;
            }
        }

        return Task.CompletedTask;
    }
}
```

### Threading

If the application has no long running loop but instead only spawns separate threads/tasks in `StartAsync`, the application would terminate and take down the threads automatically. To prevent this, use `RunWithArgumentsAsync`.

Then, in the `StopAsync` method, implement logic to clean up, and terminate threads to allow the application to stop correctly.

#### Example

```csharp
public class ConsoleRunner : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) {
        // Spawn threads...

        // Return without blocking. Threads are still up!
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        // Stop threads and wait for all to have stopped.
        return Task.CompletedTask;
    }
}
```
