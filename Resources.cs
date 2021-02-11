using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public static readonly string BiomesExtention = "*.biome.json";
        public static readonly string TerrainLayerExtention = "*.layer.json";

        public static readonly DirectoryInfo BiomesFolder = Directory.CreateDirectory(BiomesFolderPath);

        internal static Dictionary<Type, Dictionary<string, UnityEngine.Object>> userResources = new Dictionary<Type, Dictionary<string, UnityEngine.Object>>();
        internal static Dictionary<Type, Dictionary<string, UnityEngine.Object>> gameResources = new Dictionary<Type, Dictionary<string, UnityEngine.Object>>();

        internal static readonly string AssetsTag = "/Assets";
        internal static readonly string MetaTag = AssetsTag + "/Meta";
        internal static readonly string AudioTag = AssetsTag + "/Audio";
        internal static readonly string TexturesTag = AssetsTag + "/Textures";
        internal static readonly string TerrainLayersTag = AssetsTag + "/TerrainLayers";

        static void LogAsset(string value, string tag = "")
        {
            Console.WriteLine(string.Format("[Nuterra.Biomes{0}] {1}", tag, value));
        }

        static void LogFileError(FileInfo file, string e, string tag = "")
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

        public static void AddObjectToResources<T>(Type type, T obj, string name) where T : UnityEngine.Object
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

        public static void AddObjectToResources<T>(T obj, string name) where T : UnityEngine.Object
        {
            AddObjectToResources<T>(typeof(T), obj, name);
        }

        public static bool ResourcesContainsKey(Type type, string name)
        {
            return userResources.ContainsKey(type) && userResources[type].ContainsKey(name);
        }

        public static bool ResourcesContainsKey<T>(string name)
        {
            return ResourcesContainsKey(typeof(T), name);
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

        public static T GetObjectFromGameResources<T>(string name) where T : UnityEngine.Object
        {
            return (T)GetObjectFromGameResources(typeof(T), name);
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

        public static IEnumerator LoadAllTextures()
        {
            var PNGs = BiomesFolder.GetFiles("*.png", SearchOption.AllDirectories);
            LogAsset("Loading PNGs", TexturesTag);
            foreach (var file in PNGs)
            {
                var name = file.Name;
                if (!ResourcesContainsKey<Texture>(name))
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(file.FullName);
                        var tex = new Texture2D(0, 0);
                        tex.LoadImage(bytes);
                        tex.Apply();

                        AddObjectToResources<Texture2D>(tex, name);
                        AddObjectToResources<Texture>(tex, name);
                    }
                    catch (Exception e)
                    {
                        LogFileError(file, e.ToString(), TexturesTag);
                    }
                    yield return null;
                }
                else
                {
                    LogAsset(string.Format("Texture \"{0}\" already exists!", name), TexturesTag);
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
                if (!ResourcesContainsKey<AudioClip>(name))
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
                                AddObjectToResources(clip, name);
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
                    LogAsset(string.Format("Audio \"{0}\" already exists!", name), AudioTag);
                }
            }

            yield break;
        }

        public static IEnumerator LoadAllTerrainLayers()
        {
            var layers = BiomesFolder.GetFiles(TerrainLayerExtention, SearchOption.AllDirectories);
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
                        if (!ResourcesContainsKey<TerrainLayer>(name))
                        {
                            var layer = layerJSON.ToObject<TerrainLayer>(JsonSerializer.CreateDefault(new JsonSerializerSettings()
                            {
                                Converters = {
                                    new JsonConverters.UnityObjectConverter<Texture>(),
                                },
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            }));

                            AddObjectToResources(layer, name);
                        }
                        else
                        {
                            LogAsset(string.Format("TerrainLayer \"{0}\" already exists!", name), TerrainLayersTag);
                        }
                    }
                    else
                    {
                        LogAsset(string.Format("TerrainLayer in file \"{0}\" has no name!", fileName), TerrainLayersTag);
                    }
                }
                catch (Exception e)
                {
                    LogFileError(file, e.ToString(), TerrainLayersTag);
                }

                yield return null;
            }
        }
    }
}
