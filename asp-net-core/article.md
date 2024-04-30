---
status: playground
---

# Environment Variables in ASP .NET Core

Let's add just one line

```cs
app.Logger.LogInformation("Playing {guitar}", builder.Configuration["Guitar"]);
```

```sh
export GUITAR=LesPaul && dotnet run && unset GUITAR
```

## Priority



## Special Environment Variables

Let's remove `Properties` folder to get a pure environment not influenced by the `launchSettings`.

There's a few [Host variables](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0#host-variables) out there. 
But we are going to focus on `ASPNETCORE_ENVIRONMENT` and `ASPNETCORE_URLS`. 

There's a few environment variables used by `ASP .NET Core` itself. Let's try

```sh
export ASPNETCORE_URLS=http://+:5100 URLS=http://+:5110 && dotnet run && unset ASPNETCORE_URLS URLS
```

```sh
export ASPNETCORE_CONTENTROOT=RockMe && dotnet run && unset ASPNETCORE_CONTENTROOT
```

```sh
export URLS=http://+:5100 && dotnet run && unset URLS
```


Note, that there's one anomally specifically with the `ENVIRONMENT` variable. What would you expect the `Hosting Environment` to be here:

```sh
export ENVIRONMENT=Wembley ASPNETCORE_ENVIRONMENT=Carnegie && dotnet run && UNSET ENVIRONMENT ASPNETCORE_ENVIRONMENT
```

What about

```sh
export ENVIRONMENT=Wembley && dotnet run && unset ENVIRONMENT
```

If you run the script you'll find that it's `Carnegie` and `Production` accordingly. So the unprefixed variable value is completely ignore. The special behaviour of `ENVIRONMENT` variable is [confirmed](https://github.com/dotnet/aspnetcore/issues/55379#issuecomment-2081539608) to be intentional. `URLS` works fine, by the way.

Not that hosting environment doesn't change. The difference is [confirmed](https://github.com/dotnet/aspnetcore/issues/55379#issuecomment-2081539608) to be an intentional behaviour.


```sh
dontet run --environment=Wembley
```




And it's [confirmed](https://github.com/dotnet/aspnetcore/issues/55379#issuecomment-2081539608) to be an intentional behaviour.

## Sections

```export
Band__LeadSinger=Kurt Cobain
```

### AddFluentEnvironmentVariables

Here's the code:

```cs

```

You can use it via a nuget package:

```cs

```