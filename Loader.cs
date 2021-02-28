using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Nuterra.Biomes
{
    public class Loader : MonoBehaviour
    {
        public IEnumerator Start()
        {
            Resources.LogAsset("Asset loading started", Resources.MetaTag);
            yield return StartCoroutine(Resources.LoadAllTextures());
            //yield return StartCoroutine(Resources.LoadAllMeshes());
            //yield return StartCoroutine(Resources.LoadAllAudioClips());
            yield return StartCoroutine(Resources.LoadAllTerrainLayers());
            yield return StartCoroutine(Resources.LoadAllMapGenerators());
            yield return StartCoroutine(Resources.LoadAllBiomes());
            yield return StartCoroutine(Resources.LoadAllBiomeGroups());
            Resources.LogAsset("Asset loading ended", Resources.MetaTag);

            /*Resources.LogAsset("Biome injection started", Resources.MetaTag);
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

            Resources.LogAsset("Biome injection ended", Resources.MetaTag);*/

            if (Resources.biomeWrappers.Count > 0 || Resources.userResources.ContainsKey(typeof(BiomeGroup)) && Resources.userResources[typeof(BiomeGroup)].Count > 0)
            {
                var m_ModePlayPrefab = typeof(UIGameMode).GetField("m_ModePlayPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
                var m_ModePlayPrefabNoTwitter = typeof(UIGameMode).GetField("m_ModePlayPrefabNoTwitter", BindingFlags.NonPublic | BindingFlags.Instance);

                var UIGameModes = UnityEngine.Resources.FindObjectsOfTypeAll<UIGameMode>().ToList();

                var CampaignMode = UIGameModes.Find(gm => gm.name == "Campaign Mode");
                var CreativeMode = UIGameModes.Find(gm => gm.name == "Creative Mode");

                var buttonPrefab = DefaultControls.CreateButton(default(DefaultControls.Resources));
                var button = buttonPrefab.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    Bootstrapper.selector.useGUILayout = true;
                });
                var text = buttonPrefab.GetComponentInChildren<Text>();
                text.text = "Select biomes";
                buttonPrefab.AddComponent<UIModeInitButton>();
                buttonPrefab.transform.SetParent(null, false);

                var containers = new object[]
                {
                    m_ModePlayPrefab.GetValue(CampaignMode),
                    m_ModePlayPrefabNoTwitter.GetValue(CampaignMode),
                    m_ModePlayPrefab.GetValue(CreativeMode),
                    m_ModePlayPrefabNoTwitter.GetValue(CreativeMode)
                };

                foreach (var item in containers)
                {
                    if (item != null && item is GameObject go)
                    {
                        var parent = go.transform.Find("Options");
                        var btn = GameObject.Instantiate(buttonPrefab);
                        btn.transform.SetParent(parent, false);

                        parent.GetComponent<GridLayoutGroup>().constraintCount += 1;
                    }
                }

                ManUI.inst.GetScreen(ManUI.ScreenType.NewGame).ScreenInitialize(ManUI.ScreenType.NewGame);
                Bootstrapper.selector.enabled = true;
            }

            yield break;
        }

        /*float rendertime = 0;
        void Update()
        {
            try
            {
                //if(Input.GetKeyDown(KeyCode.L) && Input.GetKey(KeyCode.LeftControl))
                if ((Time.time - rendertime) > 5)
                {
                    List<Color> pixels = new List<Color>();
                    Action<Color> doPixel = pixels.Add;

                    var size = new Vector2(250, 250);
                    var origin = new Vector2(Singleton.playerPos.x, Singleton.playerPos.z);// - size * 0.5f;

                    var step = 1;
                    var seed = ManWorld.inst.SeedValue;
                    var gridScale = Singleton.Manager<ManWorld>.inst.CellsPerTileEdge;
                    var cellScale = (int)Singleton.Manager<ManWorld>.inst.CellScale;
                    var renderDetail = 1;
                    var renderMode = BiomeMap.RenderMode.Weighted;

                    ManWorld.inst.CurrentBiomeMap.Render(origin, size, step, seed, gridScale, cellScale, doPixel, renderDetail, renderMode);

                    var tex_size = (int)Math.Sqrt(pixels.Count);
                    var tex = new Texture2D(tex_size, tex_size);
                    tex.SetPixels(pixels.ToArray());
                    tex.Apply();
                    var bytes = tex.EncodeToPNG();

                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BIOME_RENDER.png"), bytes);
                    rendertime = Time.time;
                }
            }
            catch { }
        }*/

        class UIModeInitButton : MonoBehaviour, UIGameModeSettings.ModeInitSettingProvider
        {
            public void AddSettings(ManGameMode.ModeSettings modeSettings) { }

            public void InitComponent()
            {
                GetComponent<Button>().onClick.AddListener(() =>
                {
                    Bootstrapper.selector.useGUILayout = true;
                });
            }
        }
    }
}
