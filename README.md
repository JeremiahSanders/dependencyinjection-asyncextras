# Dependency Injection Async Extras

> Extensions to [`Microsoft.Extensions.DependencyInjection.Abstractions`][Microsoft.Extensions.DependencyInjection.Abstractions] supporting resolving services asynchronously.
>
> This project is not affiliated with Microsoft and asserts no claims upon its intellectual property.

## Example Use Cases

### Retrieving Remote Configuration

Configuration and options are not always applied by environment variables and command line arguments. Some applications need runtime configuration which is only available asynchronously, e.g., in a database or from a web server.

By registering an async service resolver this configuration can be retrieved reliably.

### Asynchronous Initialization

Some objects are only partially ready for use when their constructor completes. These types often expose a method like `public Task InitializeAsync()` which must be executed and awaited before the object is ready to use. A common example is `xUnit` test classes, which often use `IAsyncLifetime` to provide an `async` initialization callback.

By registering an async service resolver you can ensure that all resolved instances of a dependency have already been asynchronously initialized before they're used.

### Asynchronous Authentication/Authorization

Many web APIs require authentication to access. The authentication details are often provided as a token or cookie which must be sent with each request. However, authentication tokens and cookies are often not static; they must often be requested from an authorization server shortly before being presented to the web API.

By registering an async service resolver you can perform async requests for authorization tokens while creating "clients" for web APIs.

## How to Use

> ### Prerequisite: Install NuGet Package
>
> Begin by installing the Dependency Injection Async Extras NuGet package in the target .NET project. Example using `dotnet` CLI:
>
> ```bash
> dotnet add package Jds.DependencyInjection.AsyncExtras
> ```

### Registering Async Services (e.g., in `Startup.RegisterServices()`)

Async services are registered using the `IServiceCollection` extension method, `TryAddAsyncResolver<T>(Func<IServiceProvider, Task<T>>, ServiceLifetime)`. This adds an `IAsyncResolver<T>` service registration.

#### Example Registering a Remote JSON Data Transfer Object

In this example, the following data transfer object is available, as JSON, from a web server.

```csharp
public record SharedApplicationConfiguration { public bool EnableExperimentalFeatures { get; init; } }
```

In the code below we are working in an ASP.NET Core application. Specifically, we're updating its `Startup` class, implementing `ConfigureServices(IServiceCollection)`.

The URL of the JSON data transfer object is provided in application configuration (via `IConfiguration`, from `Microsoft.Extensions.Configuration.Abstractions`). (For example, in ASP.NET Core applications, environment variables, command line arguments, and `appsettings.json` files provide the configuration, by default.)

```csharp
// This `IServiceCollection` is assumed to have an `HttpClient` and `IConfiguration` registered prior to this code (but not shown).
IServiceCollection serviceCollection;

const string RemoteServerUrlConfigurationKey = "RemoteServerUrl";

serviceCollection.TryAddAsyncResolver<SharedApplicationConfiguration>(async serviceProvider =>
{
  // Get the remote server URL which will return the needed shared application configuration.
  string remoteUrl = serviceProvider.GetRequiredService<IConfiguration>().GetValue<string>(RemoteServerUrlConfigurationKey);
  // Get an HttpClient and retrieve the remote configuration JSON.
  SharedApplicationConfiguration? sharedConfiguration = await serviceProvider.GetRequiredService<HttpClient>()
    .GetFromJsonAsync<SharedApplicationConfiguration>(remoteUrl);
  // Return the remote configuration.
  return sharedConfiguration ?? new SharedApplicationConfiguration();
});
```

### Using Async Services in Constructors

Async services are accessed in constructors by accepting an `IAsyncResolver<T>`, where `T` is the async service.

#### Example Resolving an Async Service in an ASP.NET Core Controller Constructor

In the example below, continuing the remote `SharedApplicationConfiguration` example above, an API `Controller` uses the async service we registered above, `SharedApplicationConfiguration`. The asynchronous value may be accessed by depending upon an `IAsyncResolver<>` of the async service, e.g., `IAsyncResolver<SharedApplicationConfiguration>`. That resolver provides the asynchronous service using its `GetValueAsync()` method.

```csharp
[ApiController]
[Route("[controller]")]
public class ExperimentalController : ControllerBase
{
    private readonly IAsyncResolver<SharedApplicationConfiguration> _sharedConfigurationResolver;

    public ExperimentalController(IAsyncResolver<SharedApplicationConfiguration> sharedConfigurationResolver)
    {
        _sharedConfigurationResolver = sharedConfigurationResolver;
    }

    [HttpGet(Name = "SecretFeature")]
    public async Task<IActionResult> GetSecretFeature()
    {
        SharedApplicationConfiguration sharedConfiguration = await _sharedConfigurationResolver.GetValueAsync();
        return sharedConfiguration.EnableExperimentalFeatures
            ? Ok()
            : NotFound();
    }
}
```

### Using Async Services with an `IServiceProvider`

Async services are accessed from `IServiceProvider` instances by extension method: `GetServiceAsync<T>()` and `GetRequiredServiceAsync<T>()`. (The `IAsyncResolver<T>` can be resolved directly from the `IServiceProvider`, like constructor example above. The extension methods allow direct access.)

#### Example Resolving an Async Service in an ASP.NET Core [`IHostedService`][hosted services]

In the example below, continuing the remote `SharedApplicationConfiguration` example above, an [`IHostedService`][hosted services] uses the async service we registered above, `SharedApplicationConfiguration`. The asynchronous value may be accessed directly from an `IServiceProvider` using an extension method, `GetServiceAsync<>()`.

> Use of an `IServiceProvider` dependency, rather than an explicit dependency, is often used when custom service resolution ["scopes"][scoped dependencies] are required. E.g., to resolve [scoped services][scoped dependencies].

```csharp
public class ExperimentalFeaturesInitializer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ExperimentalFeaturesInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<ExperimentalFeaturesInitializer>>();

        var sharedConfiguration = await _serviceProvider.GetRequiredServiceAsync<SharedApplicationConfiguration>();

        if (sharedConfiguration.EnableExperimentalFeatures)
        {
            logger.LogInformation("Experimental features are enabled.");
        }
    }
}
```

[hosted services]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
[Microsoft.Extensions.DependencyInjection.Abstractions]: https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Abstractions
[scoped dependencies]: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#scoped
