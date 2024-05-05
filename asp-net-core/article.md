---
status: done
---

# Environment Variables in ASP .NET Core

In this article, we are going to learn about environment variables in ASP .NET Core by organizing a small concert (in code, of course). Let's start by creating our project:

```sh
dotnet new web --name Concert
```

And updating `Program.cs`:

```cs
// replace this:
app.MapGet("/", () => "Hello World!");

// with this:
app.Logger.LogInformation("Playing {guitar} guitar", builder.Configuration["Guitar"]);
```

The setup is that easy. Let's do our first sound check, now:

```sh
cd Concert
dotnet run

# Produces:
#
# info: Concert[0]
#       Playing (null) guitar
# ...
```

Well, this will not be a particularly good concert playing `null`, right? Let's fix it by using an environment variable:

```sh
export GUITAR=LesPaul && dotnet run && unset GUITAR
# Output: Playing LesPaul guitar
```

> The script ends with `unset` to ensure we have a clean environment before the next experiment.

## IConfiguration

Note that we access `Guitar` not directly as an environment variable, but by using an `IConfiguration` accessor abstraction. By default in `ASP .NET Core` the accessor provides us with two more ways to pick a guitar with environment variables:

`ASPNETCORE_` prefixed variables:

```sh
export ASPNETCORE_GUITAR=Telecaster && dotnet run && unset ASPNETCORE_GUITAR 
# Output: Playing Telecaster Guitar
```

And `DOTNET_` prefixed variables

```sh
export DOTNET_GUITAR=SG && dotnet run && unset DOTNET_GUITAR 
# Output: Playing SG guitar
```

If you wonder what will happen if we use both, here's the answer:

```sh
export ASPNETCORE_GUITAR=Telecaster DOTNET_GUITAR=SG && dotnet run && unset ASPNETCORE_GUITAR DOTNET_GUITAR 
# Output: Playing SG guitar
# DOTNET_ prefixed variables take precedence
```

Of course, the `IConfiguration` is not limited to environment variables. `appsettings.json` also can provide us with configuration values, so let's set a guitar there too:

```json
{
  "Guitar" : "Stratocaster",
  ...
}
```

And run a few experiments:

```sh
export DOTNET_GUITAR=SG && dotnet run && unset DOTNET_GUITAR 
# Output: Playing Stratocaster guitar
# appsettings take precedence over prefixed environment variables
```

```sh
export GUITAR=LesPaul && dotnet run && unset GUITAR 
# Output: Playing LesPaul guitar
# Unprefixed environment variable takes precedence over appsettings
```

One more way to set a configuration value is by using command line arguments. We already have our `appsettings` values in place, let's also set environment variables, provide a command line argument, and see what happens:

```sh
export GUITAR=LesPaul && dotnet run --Guitar=Firebird && unset GUITAR 
# Output: Playing Firebird guitar
# command line arguments take precedence over everything
```

I want to highlight that the priority and the list of configuration sources are not really magical. That's just a way `WebApplication.CreateBuilder(args)` registers its configuration sources. So we can

```cs
configuration.AddJsonFile("appsettings.json");
configuration.AddJsonFile($"appsettings.{HostEnvironment.EnvironmentName}.json", optional: true);
configuration.AddEnvironmentVariables(prefix: "ASPNETCORE_");
configuration.AddEnvironmentVariables(prefix: "DOTNET_");
configuration.AddEnvironmentVariables();
configuration.AddCommandLine(args);
```

## Special Environment Variables

Let's remove `Properties` folder to get a pure environment not influenced by the `launchSettings`.



There are also a few environment variables used by `ASP .NET Core` itself. To set up a clear experiment first, let's delete `Properties` folder from the project. Then executing `dotnet run` will get us such logs:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /Users/egortarasov/fluenv/asp-net-core/Concert
```

There are a few [Host variables](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0#host-variables) out there. But studying `ASPNETCORE_ENVIRONMENT` and `ASPNETCORE_URLS` seems to be the most important and should give us enough knowledge to operate fluently with any hosting variable.

```sh
export ASPNETCORE_URLS=http://+:5100 && dotnet run && unset ASPNETCORE_URLS
# Outputs: Now listening on: http://[::]:5100

export ASPNETCORE_ENVIRONMENT=Wembley && dotnet run && unset ASPNETCORE_ENVIRONMENT
# Outputs: Hosting environment: Wembley
```

Note, that host variables may behave a little differently from all the other variables:

```sh
export ENVIRONMENT=Carnegie && dotnet run && unset ENVIRONMENT
# Outputs: Hosting environment: Production
# Unprefixed variable has no effect on the ASP .NET Core
```

This difference is [confirmed](https://github.com/dotnet/aspnetcore/issues/55379#issuecomment-2081539608) to be intentional. But not every host variable behaves that way, only the "bootstrap" variables:

```sh
export URLS=http://+:5800 && dotnet run && unset URLS
# Outputs: Now listening on: http://[::]:5800
# Here unprefixed variables not just affect ASP .NET Core
# but take precedence over a prefixed variable
```

## Sections And Underscored

`Microsoft.Extensions.Configuration` framework supports nested configuration, as well. Let's first see how it would work with json based configuration.

`appsettings.json`:

```json
{
    "Band" : {
        "LeadGuitarist" : "Clapton"
    },
    ...
}
```

`Program.cs`:

```cs
app.Logger.LogInformation("{guitarist} playing {guitar}",
    builder.Configuration["Band:LeadGuitarist"],
    builder.Configuration["Guitar"]
);

//Output: Clapton playing Stratocaster
```

For "nesting" environment variables double underscore is used:


```sh
export Band__LeadGuitarist=Hendrix && dotnet run && unset Band__LeadGuitarist
# Output: Hendrix playing Stratocaster
```

> Note that double underscore `__` is used because `:` is not a valid identifier for certain shells, including bash.

### Fluent environment variables

You may notice that `Band__LeadGuitarist` is a variable name that doesn't really follow the typical shell convention. The conventional form would be: `BAND_LEAD_GUITARIST`. And there's good news about the environment variables configuration provider:

```sh
export BAND__LEADGUITARIST=Hendrix && dotnet run && unset BAND__LEADGUITARIST
# Output: Hendrix playing Stratocaster
# So the provider is case incensitive
```

But one good news is not quite enough to make it:

```sh
export Band_LeadGuitarist=Gilmour && dotnet run && unset Band_LeadGuitarist
# Ouput: Clapton playing Stratocaster (a.k.a no effect)
# Single underscore doesn't work as separator

export Band__Lead_Guitarist=Gilmour && dotnet run && unset Band__Lead_Guitarist
# Ouput: Clapton playing Stratocaster (a.k.a no effect)
# You can not put an arbitrary underscore, too
```

However, we can write our own configuration provider. For each environment variable key we'll register the key itself and keys for each possible interpretation of underscore (as separator and as skippable part):

```cs
public static IEnumerable<string> Keys(string rawKey)
{
      yield return rawKey;

      var parts = rawKey.Split("_").Where(p => p != "").ToArray();

      for (var i = 1; i < parts.Length; i++)
      {
            var beforeColon = parts.Take(i);
            var afterColon = parts.Skip(i);

            yield return String.Join("", beforeColon) + ":" + String.Join("", afterColon);
      }
}
```

And the provider will load all the configuration key-value pairs we can get from the environment variables.

```cs
public class Provider : ConfigurationProvider
{
      public override void Load()
      {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
            {
                var variableKey = (string)environmentVariable.Key;
                var value = (string?)environmentVariable.Value;

                foreach (var key in Keys(variableKey))
                {
                    Data.Add(key, value);
                }
            }
      }
}
```

I've already made the provider as a nuget package, so you can just use it:

```sh
dotnet add package Fluenv
```

```cs
using Fluenv;

...

builder.Configuration.AddFluentEnvironmentVariables();
```

Then virtually any naming of environment variables will work, including the conventional one:

```sh
export BAND_LEAD_GUITARIST=Gilmour && dotnet run && unset BAND_LEAD_GUITARIST
```
