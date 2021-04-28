# .Net 5 console application demo

A console app built using .NET 5 with support to Microsoft best practices to show how to setup:

- Multiple environments,
- Configuration values using the `Options pattern`,
- Appsettings,
- Usersecrets,
- Azure KeyVault,
- Azure Application Insight,
- ConsoleHostedService,
- HttpClientFactory,

## Setup

### Environments

It is possible to configure multiple environments (Development, Test, Production, etc) in different ways.
Then it's possible to run with a specific environment configuration, as example:

1. `Project` >  `Properties` > `Debug` > `Environment variables` > Name: `DOTNET_ENVIRONMENT`, Value: `Development`
2. `dotnet run --environment=Development`
3. `Properties` > `launchSettings.json`

Hence you can create different `appsettings.ENV.json` files, like `appsettings.development.json`.

NB. `context.HostingEnvironment.IsDevelopment()` requires an environment called `Development` (and not just `Dev`)

The configuration files can contain only partial object, and can override eachother, following the order:

- `appsettings.json`
- `appsettings.ENV.json`
- `secrets.json`
- Azure KeyVault

Your custom configuration properties (Options pattern) should be placed inside objects, and not added directly to the AppConfig class.
In this wy it's possible to inject these classes during DI.

### User secrets

It's possible to store sensible configuration in `secrets.json` (added easily via Visual Studio) and in this configuration it's mandatory.

### Azure KeyVault

Configure in your subscription an Azure KeyVault and copy the KeyVault uri in the config file:

`KeyVaultUri: "https://your-key-vault-name.vault.azure.net/"`

### Azure Application Insight

Configure in your subscription an Azure Application Insight and copy the InstrumentationKey (a GUID) in the config file:

`InstrumentationKey: "your application insight guid"`.

We will also configure a `TelemetryClient`, but be aware that `Flush()` is not a blocking operation and it doesn't guarantee that the logs are sent to the Azure backplane. That means we need to add a Sleep as suggested from the docs, waiting for a proper fix in the AI package.
NB. There is not an ufficial sleep-time suggested, so tweak this value as you need.

### Configure logging

The logging level can be configured in the config files.

### Dumping the built configuration

It's possible, just for debugging purposes, to dump out the configuration tree.
NB. do not use in production, neither expose publicly, as it is a security risk.

## Dependency Injection (DI)

The goal of this demo/template is to show how it's possible to support DI for every dependency. One example is `IHttpClientFactory` that allows to build `HttpClients` in a memory-safe way, adds many features (like named instances), is very flexible, and can be configured in many ways.

## HostedService

The staring point of the console is a `HostedService`, a Singleton that the framework will manage for us, and will take care of calling `StartAsync` and `StopAsync` in the right moment in the application lifecycle, allowing also gracefully shutdowns.

Inside the ConsoleHostedService it's possible to use services with a different scope, like a `Transient` or a `Scoped` service (es. `EntityFramework`), using `IServiceScopeFactory`.
