using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Harmony;

namespace Nuterra.Biomes
{
    public class Bootstrapper
    {
        public static void Load()
        {
            var harmony = HarmonyInstance.Create("nuterra.biomes");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
