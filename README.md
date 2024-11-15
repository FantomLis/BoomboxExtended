# Content Warning Harmony Template

Thank you for using the mod template! Here are a few tips to help you on your journey:

## Versioning

BepInEx uses [semantic versioning, or semver](https://semver.org/), for the mod's version info.
To increment it, you can either modify the version tag in the `.csproj` file directly, or use your IDE's UX to increment the version. Below is an example of modifying the `.csproj` file directly:

```xml
<!-- BepInEx Properties -->
<PropertyGroup>
    <AssemblyName>FantomLis.BoomboxExtended</AssemblyName>
    <Product>BoomboxdotNetStandartPort</Product>
    <!-- Change to whatever version you're currently on. -->
    <Version>1.0.0</Version>
</PropertyGroup>
```

Your IDE will have the setting in `Package` or `NuGet` under `General` or `Metadata`, respectively.

## Logging

A logger is provided to help with logging to the console.
You can access it by doing `Plugin.Logger` in any class outside the `Plugin` class.

***Please use*** `LogDebug()` ***whenever possible, as any other log method
will be displayed to the console and potentially cause performance issues for users.***

If you chose to do so, make sure you change the following line in the `BepInEx.cfg` file to see the Debug messages:

```toml
[Logging.Console]

# ... #

## Which log levels to show in the console output.
# Setting type: LogLevel
# Default value: Fatal, Error, Warning, Message, Info
# Acceptable values: None, Fatal, Error, Warning, Message, Info, Debug, All
# Multiple values can be set at the same time by separating them with , (e.g. Debug, Warning)
LogLevels = All
```

## Harmony

This template uses harmony. For more specifics on how to use it, look at
[the HarmonyX GitHub wiki](https://github.com/BepInEx/HarmonyX/wiki) and
[the Harmony docs](https://harmony.pardeike.net/).

To make a new harmony patch, just use `[HarmonyPatch]` before any class you make that has a patch in it.

Then in that class, you can use
`[HarmonyPatch(typeof(ClassToPatch), "MethodToPatch")]`
where `ClassToPatch` is the class you're patching (ie `ShoppingCart`), and `MethodToPatch` is the method you're patching (ie `AddItemToCart`).

Then you can use
[the appropriate prefix, postfix, transpiler, or finalizer](https://harmony.pardeike.net/articles/patching.html) attribute.

_While you can use_ `return false;` _in a prefix patch,
it is **HIGHLY DISCOURAGED** as it can **AND WILL** cause compatibility issues with other mods._

For example, we want to add a patch that will add a random amount to the total value of items in the cart upon adding an item.
We have the following postfix patch patching the `AddItemToCart` method
in `ShoppingCart`:

```csharp
using System;
using System.Reflection;
using HarmonyLib;

namespace BoomboxdotNetStandartPort.Patches;

[HarmonyPatch(typeof(ShoppingCart))]
public class ExampleShoppingCartPatch
{
    [HarmonyPatch("AddItemToCart")]
    [HarmonyPostfix]
    private static void AddItemToCartPostfix(ShoppingCart __instance)
    {
        /*
         * Adding a random value to the visible price of the shopping cart (not actual) is slightly complicated
         * due to the private setter of the CartValue property. So to change the value, we must get the setter
         * via reflection, and call it with the new value.
         */
        AccessTools.PropertySetter(typeof(ShoppingCart), "CartValue").Invoke(__instance, ShoppingCart.CartValue + new Random().Next(0, 100));
    }
}

```

In this case we include the type of the class we're patching in the attribute
before our `ExampleShoppingCartPatch` class,
as our class will only patch the `ShoppingCart` class.
