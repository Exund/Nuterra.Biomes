using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Nuterra.Biomes
{
    public static class Resources
    {
        public static readonly string BiomesFolderPath = Path.Combine(Application.dataPath, "../Custom Biomes");
        public static readonly string BiomesExtension = ".biome.json";
        public static readonly string TerrainLayerExtension = ".layer.json";
        public static readonly string MapGeneratorExtension = ".generator.json";
        public static readonly string BiomeGroupsExtension = ".group.json";

        public static readonly DirectoryInfo BiomesFolder = Directory.CreateDirectory(BiomesFolderPath);

        internal static Dictionary<Type, Dictionary<string, UnityEngine.Object>> userResources = new Dictionary<Type, Dictionary<string, UnityEngine.Object>>();
        internal static Dictionary<Type, Dictionary<string, UnityEngine.Object>> gameResources = new Dictionary<Type, Dictionary<string, UnityEngine.Object>>();
        internal static List<BiomeWrapper> biomeWrappers = new List<BiomeWrapper>();

        internal static readonly string AssetsTag = "/Assets";
        internal static readonly string MetaTag = AssetsTag + "/Meta";
        internal static readonly string AudioTag = AssetsTag + "/Audio";
        internal static readonly string TexturesTag = AssetsTag + "/Textures";
        internal static readonly string TerrainLayersTag = AssetsTag + "/TerrainLayers";
        internal static readonly string MapGeneratorsTag = AssetsTag + "/MapGenerators";
        internal static readonly string BiomesTag = AssetsTag + "/Biomes";
        internal static readonly string BiomeGroupsTag = AssetsTag + "/BiomeGroups";

        internal static Type BiomeGroup_T = typeof(BiomeGroup);
        internal static FieldInfo[] BiomeGroup_fields = BiomeGroup_T.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        internal static void LogAsset(string value, string tag = "")
        {
            Console.WriteLine(string.Format("[Nuterra.Biomes{0}] {1}", tag, value));
        }

        internal static void LogError(string value, string tag = "")
        {
            LogAsset("Error: " + value, tag);
        }

        internal static void LogFileError(FileInfo file, string e, string tag = "")
        {
            LogAsset(string.Format("Error while loading \"{0}\" :\n{1}", Path.Combine(file.Directory.Name, file.Name), e), tag);
        }

        public static string StripComments(string input)
        {
            // JavaScriptSerializer doesn't accept commented-out JSON,
            // so we'll strip them out ourselves;
            // NOTE: for safety and simplicity, we only support comments on their own lines,
            // not sharing lines with real JSON
            input = Regex.Replace(input, @"^\s*//.*$", "", RegexOptions.Multiline);  // removes comments like this
            input = Regex.Replace(input, @"^\s*/\*(\s|\S)*?\*/\s*$", "", RegexOptions.Multiline); /* comments like this */

            return input;
        }

        public static void AddObjectToUserResources(Type type, UnityEngine.Object obj, string name)
        {
            if (!userResources.ContainsKey(type))
            {
                userResources.Add(type, new Dictionary<string, UnityEngine.Object>());
            }
            /*if (userResources[type].ContainsKey(name))
            {
                userResources[type][name] = obj;
            }
            else
            {*/
            userResources[type].Add(name, obj);
            //}

            LogAsset(string.Format("Added {0} {1}", type.Name, name), MetaTag);
        }

        public static void AddObjectToUserResources<T>(T obj, string name) where T : UnityEngine.Object
        {
            AddObjectToUserResources(typeof(T), obj, name);
        }

        public static bool UserResourcesContainsKey(Type type, string name)
        {
            return userResources.ContainsKey(type) && userResources[type].ContainsKey(name);
        }

        public static bool UserResourcesContainsKey<T>(string name)
        {
            return UserResourcesContainsKey(typeof(T), name);
        }

        public static T GetObjectFromUserResources<T>(string name) where T : UnityEngine.Object
        {
            return (T)GetObjectFromUserResources(typeof(T), name);
        }

        public static UnityEngine.Object GetObjectFromUserResources(Type type, string name)
        {
            if (userResources.TryGetValue(type, out var bucket) && bucket.TryGetValue(name, out var item))
            {
                return item;
            }

            return null;
        }

        public static UnityEngine.Object GetObjectFromGameResources(Type type, string name)
        {
            if (gameResources.TryGetValue(type, out var CacheLookup))
            {
                if (CacheLookup.TryGetValue(name, out var result))
                    return result;
            }
            else
            {
                gameResources.Add(type, new Dictionary<string, UnityEngine.Object>());
            }

            UnityEngine.Object searchresult = null;
            var search = UnityEngine.Resources.FindObjectsOfTypeAll(type);
            foreach (var item in search)
            {
                if (item.name == name)
                {
                    searchresult = item;
                    break;
                }
            }
            if (searchresult == null)
            {
                foreach (var item in search)
                {
                    if (item.name.StartsWith(name))
                    {
                        searchresult = item;
                        break;
                    }
                }
            }

            gameResources[type].Add(name, searchresult);
            return searchresult;
        }

        public static T GetObjectFromGameResources<T>(string name) where T : UnityEngine.Object
        {
            return (T)GetObjectFromGameResources(typeof(T), name);
        }

        public static UnityEngine.Object GetObjectFromResources(Type type, string name)
        {
            var res = GetObjectFromUserResources(type, name);

            if (!res)
            {
                res = GetObjectFromGameResources(type, name);
            }

            return res;
        }

        public static T GetObjectFromResources<T>(string name) where T : UnityEngine.Object
        {
            return (T)GetObjectFromResources(typeof(T), name);
        }


        public static IEnumerator LoadAllTextures()
        {
            var PNGs = BiomesFolder.GetFiles("*.png", SearchOption.AllDirectories);
            LogAsset("Loading PNGs", TexturesTag);
            foreach (var file in PNGs)
            {
                var name = file.Name;
                if (!UserResourcesContainsKey<Texture>(name))
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(file.FullName);
                        var tex = new Texture2D(0, 0);
                        tex.LoadImage(bytes);
                        tex.Apply();

                        AddObjectToUserResources<Texture2D>(tex, name);
                        AddObjectToUserResources<Texture>(tex, name);
                    }
                    catch (Exception e)
                    {
                        LogFileError(file, e.ToString(), TexturesTag);
                    }
                    yield return null;
                }
                else
                {
                    LogError(string.Format("Texture \"{0}\" already exists!", name), TexturesTag);
                }
            }

            yield break;
        }

        public static IEnumerator LoadAllAudioClips()
        {
            var MP3s = BiomesFolder.GetFiles("*.mp3", SearchOption.AllDirectories);
            LogAsset("Loading MP3s", AudioTag);
            yield return LoadAudioClipsFromFiles(MP3s, AudioType.MPEG);

            var WAVs = BiomesFolder.GetFiles("*.wav", SearchOption.AllDirectories);
            LogAsset("Loading WAVs", AudioTag);
            yield return LoadAudioClipsFromFiles(WAVs, AudioType.WAV);

            yield break;
        }

        static IEnumerator LoadAudioClipsFromFiles(FileInfo[] files, AudioType type)
        {
            foreach (var file in files)
            {
                var name = file.Name;
                if (!UserResourcesContainsKey<AudioClip>(name))
                {
                    var path = Path.Combine("file://", file.FullName);
                    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, type))
                    {
                        yield return www.SendWebRequest();

                        try
                        {
                            if (www.isNetworkError)
                            {
                                LogFileError(file, www.error, AudioTag);
                            }
                            else
                            {
                                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                                AddObjectToUserResources(clip, name);
                            }
                        }
                        catch (Exception e)
                        {
                            LogFileError(file, e.ToString(), AudioTag);
                        }
                    }
                }
                else
                {
                    LogError(string.Format("Audio \"{0}\" already exists!", name), AudioTag);
                }
            }

            yield break;
        }

        public static IEnumerator LoadAllTerrainLayers()
        {
            var layers = BiomesFolder.GetFiles("*" + TerrainLayerExtension, SearchOption.AllDirectories);
            LogAsset("Loading TerrainLayers", TerrainLayersTag);

            foreach (var file in layers)
            {
                var fileName = file.Name;
                try
                {
                    var layerJSON = JObject.Parse(File.ReadAllText(file.FullName));

                    if (layerJSON["name"] != null)
                    {
                        var name = layerJSON["name"].ToString();
                        if (!UserResourcesContainsKey<TerrainLayer>(name))
                        {
                            var layer = layerJSON.ToObject<TerrainLayer>(JsonSerializer.CreateDefault(new JsonSerializerSettings()
                            {
                                Converters = {
                                    new JsonConverters.UnityObjectConverter<Texture>(),
                                },
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            }));

                            AddObjectToUserResources(layer, name);
                        }
                        else
                        {
                            LogError(string.Format("TerrainLayer \"{0}\" already exists!", name), TerrainLayersTag);
                        }
                    }
                    else
                    {
                        LogError(string.Format("TerrainLayer in file \"{0}\" has no name!", fileName), TerrainLayersTag);
                    }
                }
                catch (Exception e)
                {
                    LogFileError(file, e.ToString(), TerrainLayersTag);
                }

                yield return null;
            }

            yield break;
        }

        public static IEnumerator LoadAllMapGenerators()
        {
            var m_Layers = typeof(MapGenerator).GetField("m_Layers", BindingFlags.Instance | BindingFlags.NonPublic);
            var generators = BiomesFolder.GetFiles("*" + MapGeneratorExtension, SearchOption.AllDirectories);
            LogAsset("Loading MapGenerators", MapGeneratorsTag);

            GameObject generators_holder = new GameObject();

            foreach (var file in generators)
            {
                var fileName = file.Name;
                try
                {
                    var generatorJSON = JObject.Parse(File.ReadAllText(file.FullName));

                    if (generatorJSON["name"] != null)
                    {
                        var name = generatorJSON["name"].ToString();
                        if (!UserResourcesContainsKey<MapGenerator>(name))
                        {
                            var generator_go = new GameObject(name);
                            generator_go.transform.SetParent(generators_holder.transform);
                            var generator_base = generator_go.AddComponent<MapGenerator>();
                            generator_base.name = name;
                            JsonConvert.PopulateObject(generatorJSON.ToString(), generator_base, new JsonSerializerSettings()
                            {
                                MissingMemberHandling = MissingMemberHandling.Ignore,
                                Converters = {
                                    new Newtonsoft.Json.Converters.StringEnumConverter()
                                    {
                                        AllowIntegerValues = true
                                    }
                                }
                            });

                            var layers = ((JArray)generatorJSON["m_Layers"]).ToObject<MapGenerator.Layer[]>();

                            m_Layers.SetValue(generator_base, layers);

                            AddObjectToUserResources(generator_base, name);
                        }
                        else
                        {
                            LogError(string.Format("MapGenerator \"{0}\" already exists!", name), MapGeneratorsTag);
                        }
                    }
                    else
                    {
                        LogError(string.Format("MapGenerator in file \"{0}\" has no name!", fileName), MapGeneratorsTag);
                    }
                }
                catch (Exception e)
                {
                    LogFileError(file, e.ToString(), MapGeneratorsTag);
                }

                yield return null;
            }

            yield break;
        }

        public static IEnumerator LoadAllBiomes()
        {
            var Biome_T = typeof(Biome);
            var fields = Biome_T.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var biomes = BiomesFolder.GetFiles("*" + BiomesExtension, SearchOption.AllDirectories);
            LogAsset("Loading Biomes", BiomesTag);

            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Converters = {
                    new JsonConverters.ScriptableObjectConverter(),
                    new JsonConverters.UnityObjectConverter<AudioClip>(),
                    new JsonConverters.UnityObjectConverter<TerrainLayer>(),
                    new JsonConverters.UnityObjectConverter<MapGenerator>(),
                    new JsonConverters.UnityObjectConverter<TerrainObject>(),
                    new JsonConverters.PrefabGroupConverter(),
                    new Newtonsoft.Json.Converters.StringEnumConverter()
                    {
                        AllowIntegerValues = true
                    }
                }
            };

            var serializer = JsonSerializer.CreateDefault(settings);

            foreach (var file in biomes)
            {
                var fileName = file.Name;
                try
                {
                    var biomeJSON = JObject.Parse(File.ReadAllText(file.FullName));

                    if (biomeJSON["name"] != null)
                    {
                        var name = biomeJSON["name"].ToString();
                        if (!UserResourcesContainsKey<Biome>(name))
                        {
                            var biome = ScriptableObject.CreateInstance<Biome>();

                            if (biomeJSON["Reference"] != null)
                            {
                                var refName = biomeJSON["Reference"].ToString();
                                var reference = GetObjectFromGameResources<Biome>(refName);
                                if (!reference)
                                {
                                    LogError(string.Format("Biome reference \"{0}\" for Biome \"{1}\" doesn't exists!", refName, name), BiomesTag);
                                    continue;
                                }

                                biome = UnityEngine.Object.Instantiate(reference);
                            }

                            biome.name = name;

                            foreach (var field in fields)
                            {
                                try
                                {
                                    if (biomeJSON[field.Name] != null)
                                    {
                                        field.SetValue(biome, biomeJSON[field.Name].ToObject(field.FieldType, serializer));
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(field.Name);
                                    Console.WriteLine(e);
                                }
                            }

                            if (biomeJSON["BiomeGroupName"] != null)
                            {
                                biomeWrappers.Add(new BiomeWrapper()
                                {
                                    biome = biome,
                                    biomeGroupName = biomeJSON["BiomeGroupName"].ToObject<string>(),
                                    biomeWeight = biomeJSON["BiomeWeight"] == null ? 1f : biomeJSON["BiomeWeight"].ToObject<float>()
                                });
                            }

                            AddObjectToUserResources(biome, name);
                        }
                        else
                        {
                            LogError(string.Format("Biome \"{0}\" already exists!", name), BiomesTag);
                        }
                    }
                    else
                    {
                        LogError(string.Format("Biome in file \"{0}\" has no name!", fileName), BiomesTag);
                    }
                }
                catch (Exception e)
                {
                    LogFileError(file, e.ToString(), BiomesTag);
                }

                yield return null;
            }
        }

        public static IEnumerator LoadAllBiomeGroups()
        {
            var biomeGroups = BiomesFolder.GetFiles("*" + BiomeGroupsExtension, SearchOption.AllDirectories);
            LogAsset("Loading BiomesGroups", BiomesTag);

            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Converters = {
                    new JsonConverters.UnityObjectConverter<Biome>(),
                    new JsonConverters.AnimationCurveConverter()
                }
            };

            var serializer = JsonSerializer.CreateDefault(settings);

            foreach (var file in biomeGroups)
            {
                var fileName = file.Name;
                try
                {
                    var biomeGroupJSON = JObject.Parse(File.ReadAllText(file.FullName));

                    if (biomeGroupJSON["name"] != null)
                    {
                        var name = biomeGroupJSON["name"].ToString();
                        if (!UserResourcesContainsKey<BiomeGroup>(name))
                        {
                            var biomeGroup = ScriptableObject.CreateInstance<BiomeGroup>();
                            biomeGroup.name = name;

                            foreach (var field in BiomeGroup_fields)
                            {
                                try
                                {
                                    if (biomeGroupJSON[field.Name] != null)
                                    {
                                        field.SetValue(biomeGroup, biomeGroupJSON[field.Name].ToObject(field.FieldType, serializer));
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(field.Name);
                                    Console.WriteLine(e);
                                }
                            }

                            AddObjectToUserResources(biomeGroup, name);
                        }
                        else
                        {
                            LogError(string.Format("BiomeGroup \"{0}\" already exists!", name), BiomeGroupsTag);
                        }
                    }
                    else
                    {
                        LogError(string.Format("BiomeGroup in file \"{0}\" has no name!", fileName), BiomeGroupsTag);
                    }
                }
                catch (Exception e)
                {
                    LogFileError(file, e.ToString(), BiomeGroupsTag);
                }

                yield return null;
            }
        }
    }
}
