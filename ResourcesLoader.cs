using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Nuterra.Biomes
{
    public class ResourcesLoader : MonoBehaviour
    {
        public IEnumerator Start()
        {
            Resources.LogAsset("Asset loading started", Resources.MetaTag);
            yield return StartCoroutine(Resources.LoadAllTextures());
            //yield return StartCoroutine(Resources.LoadAllAudioClips());
            yield return StartCoroutine(Resources.LoadAllTerrainLayers());
            yield return StartCoroutine(Resources.LoadAllMapGenerators());
            yield return StartCoroutine(Resources.LoadAllBiomes());
            yield return StartCoroutine(Resources.LoadAllBiomeGroups());
            Resources.LogAsset("Asset loading ended", Resources.MetaTag);

            Resources.LogAsset("Biome injection started", Resources.MetaTag);
            var m_Biomes = Resources.BiomeGroup_fields.First(f => f.Name == "m_Biomes");
            var m_BiomeWeights = Resources.BiomeGroup_fields.First(f => f.Name == "m_BiomeWeights");
            foreach (var item in Resources.biomeWrappers)
            {
                var group = Resources.GetObjectFromResources<BiomeGroup>(item.biomeGroupName);
                if (group)
                {
                    var biomes = ((Biome[])m_Biomes.GetValue(group)).ToList();
                    biomes.Add(item.biome);
                    m_Biomes.SetValue(group, biomes.ToArray());

                    var weights = ((float[])m_BiomeWeights.GetValue(group)).ToList();
                    weights.Add(item.biomeWeight);
                    m_BiomeWeights.SetValue(group, weights.ToArray());

                    Resources.LogAsset(string.Format("Biome \"{0}\" added to BiomeGroup \"{1}\"", item.biome.name, item.biomeGroupName), Resources.BiomesTag);
                }
                else
                {
                    Resources.LogError(string.Format("BiomeGroup \"{0}\" doesn't exist for Biome \"{1}\"", item.biomeGroupName, item.biome.name), Resources.BiomesTag);
                }
            }

            var bindings = BindingFlags.NonPublic | BindingFlags.Instance;
            var BiomeMap_T = typeof(BiomeMap);
            var MainBiomeMap = Resources.GetObjectFromGameResources<BiomeMap>("MainBiomeMap");
            var m_BiomeGroups = BiomeMap_T.GetField("m_BiomeGroups", bindings);
            if (Resources.userResources.TryGetValue(typeof(BiomeGroup), out var customGroups))
            {

                var groups = ((BiomeGroup[])m_BiomeGroups.GetValue(MainBiomeMap)).ToList();

                foreach (var group in customGroups)
                {
                    groups.Add((BiomeGroup)group.Value);
                }

                m_BiomeGroups.SetValue(MainBiomeMap, groups.ToArray());
                Resources.LogAsset("Custom BiomeGroups injected in MainBiomeMap", Resources.MetaTag);
            }

            try
            {
                var m_BiomeGroupDatabase = BiomeMap_T.GetField("m_BiomeGroupDatabase", bindings);
                m_BiomeGroupDatabase.SetValue(MainBiomeMap, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Resources.LogAsset("Biome injection ended", Resources.MetaTag);

            yield break;
        }

        void OnGUI()
        {
            try
            {
                if (ManGameMode.inst.GetIsInPlayableMode())
                {
                    var tile = ManWorld.inst.TileManager.LookupTile(Singleton.playerPos, false);
                    var cell = ManWorld.inst.TileManager.GetMapCell(tile, Singleton.playerPos);
                    GUI.TextField(new Rect(Screen.width / 2 - 100f, 100f, 200f, 30f), ManWorld.inst.CurrentBiomeMap.LookupBiome(cell.Index(0)).name);
                }
            }
            catch { }
        }
    }
}
