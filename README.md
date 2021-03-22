## Intorduction

It is small library which extends [AspNetCoreRateLimit](https://github.com/stefanprodan/AspNetCoreRateLimit) library.

## Purpose

1. Use MongoDB as distributed rules storage
2. Reload configuration after certain amount of time

## Usage

1. Define settings in application.json 

```json

...
    "SomeConfigSection" : 
    {
        "TargetDatabase": "some-mongo-database-name",
        "TargetCollection": "some-collection-name",
        "ReloadIntervalInSeconds": 60, // optional
    }
...

```

2. Load main configuration

```Csharp
    services.Configure<IpRateLimitMongoConfiguration>(configuration.GetSection("SomeConfigSection"));
```

3. Inject **native mongo client** in your IoC container or services
4. Inject singleton in IoC container or services as implementation of `IRateLimitConfiguration`

```Csharp
    services.AddSingleton<IRateLimitConfiguration, RateLimitConfigurationMongo>();
```

5. Your are done

## Notes

1. Main configuration will be overwritten, if `ReloadIntervalInSeconds` is set