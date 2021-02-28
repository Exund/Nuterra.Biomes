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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.logMessageReceivedThreaded += HandleLogEntry;

            Resources.LogAsset("Nuterra Biome Injector Library started");

            var holder = new GameObject();
            holder.AddComponent<ResourcesLoader>();
            holder.AddComponent<Extractor>();

            GameObject.DontDestroyOnLoad(holder);

            var harmony = HarmonyInstance.Create("nuterra.biomes");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("\n\nGAME BREAK\n" + sender.ToString() + "\n" + e.ExceptionObject.ToString() + "\n\n");
        }

        private static void HandleLogEntry(string logEntry, string stackTrace, LogType logType)
        {
            if (logType == LogType.Exception)
            {
                Console.WriteLine("\n\nGAME BREAK\n" + logEntry + "\n" + stackTrace + "\n\n");
            }
        }
    }
}
