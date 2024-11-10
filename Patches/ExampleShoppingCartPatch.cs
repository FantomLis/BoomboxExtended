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
        AccessTools.PropertySetter(typeof(ShoppingCart), "CartValue").Invoke(
            __instance, new object[] { __instance.CartValue + new Random().Next(0, 100) });
    }
}
