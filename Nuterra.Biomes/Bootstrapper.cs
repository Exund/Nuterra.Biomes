using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace Nuterra.Biomes
{
    public class Bootstrapper
    {
        static ManBiomeSave biomeSaveManager = new ManBiomeSave();
        internal static BiomeSelector selector;

        public static void Load()
        {
            Resources.LogAsset("Nuterra Biome Injector Library started");

            var holder = new GameObject();
            holder.AddComponent<Loader>();
            holder.AddComponent<Extractor>();
            selector = holder.AddComponent<BiomeSelector>();
            selector.enabled = false;
            selector.useGUILayout = false;

            GameObject.DontDestroyOnLoad(holder);

            var harmony = new Harmony("nuterra.biomes");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        static class Patches
        {
            static class ModeMain_Patches
            {
                [HarmonyPatch(typeof(ModeMain), "SetupModeLoadSaveListeners")]
                static class SetupModeLoadSaveListeners
                {
                    static void Postfix(ref ModeMain __instance)
                    {
                        __instance.SubscribeToEvents(biomeSaveManager);
                    }
                }

                [HarmonyPatch(typeof(ModeMain), "CleanupModeLoadSaveListeners")]
                static class CleanupModeLoadSaveListeners
                {
                    static void Postfix(ref ModeMain __instance)
                    {
                        __instance.UnsubscribeFromEvents(biomeSaveManager);
                    }
                }
            }

            static class ModeMisc_Patches
            {
                [HarmonyPatch(typeof(ModeMisc), "SetupModeLoadSaveListeners")]
                static class SetupModeLoadSaveListeners
                {
                    static void Postfix(ref ModeMisc __instance)
                    {
                        if(__instance.GetGameType() == ManGameMode.GameType.Creative)
                            __instance.SubscribeToEvents(biomeSaveManager);
                    }
                }

                [HarmonyPatch(typeof(ModeMisc), "CleanupModeLoadSaveListeners")]
                static class CleanupModeLoadSaveListeners
                {
                    static void Postfix(ref ModeMisc __instance)
                    {
                        if (__instance.GetGameType() == ManGameMode.GameType.Creative)
                            __instance.UnsubscribeFromEvents(biomeSaveManager);
                    }
                }
            }

            static class UIScreenNewGame_Patches
            {
                [HarmonyPatch(typeof(UIScreenNewGame), "OnBackClicked")]
                static class OnBackClicked
                {
                    static void Postfix()
                    {
                        HideSelector();
                    }
                }

                [HarmonyPatch(typeof(UIScreenNewGame), "Hide")]
                static class Hide
                {
                    static void Postfix()
                    {
                        Bootstrapper.selector.useGUILayout = false;
                    }
                }

                [HarmonyPatch(typeof(UIScreenNewGame), "OnModeClicked")]
                static class OnModeClicked
                {
                    static void Postfix()
                    {
                        HideSelector();
                    }
                }

                static void HideSelector()
                {
                    Bootstrapper.selector.Reset();
                    Bootstrapper.selector.useGUILayout = false;
                }
            }
        }
    }
}
