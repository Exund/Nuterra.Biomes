using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Nuterra.Biomes
{
    public class Resources : MonoBehaviour
    {
        public static readonly string BiomesFolderPath = Path.Combine(Application.dataPath, "../Custom Biomes");
        public static readonly string BiomesExtention = "*.biome.json";
        public static readonly string TerrainLayerExtention = "*.layer.json";

        public static DirectoryInfo BiomesFolder = Directory.CreateDirectory(BiomesFolderPath);

        private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private static Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();

        static readonly string AssetsTag = "/Assets";
        static readonly string AudioTag = AssetsTag + "/Audio";
        static readonly string TexturesTag = AssetsTag + "/Textures";

        IEnumerator textures_coroutine;
        IEnumerator sounds_coroutine;

        public void Start()
        {
            textures_coroutine = LoadAllTextures();
            sounds_coroutine = LoadAllAudioClips();
        }

        public void Update()
        {
            if(textures_coroutine != null)
            {
                bool running;
                do
                {
                    running = textures_coroutine.MoveNext();
                } while (running);
                return;
            }

            if(sounds_coroutine != null)
            {
                bool running;
                do
                {
                    running = sounds_coroutine.MoveNext();
                } while (running);
                return;
            }
        }

        public IEnumerator LoadAllTextures()
        {
            var PNGs = BiomesFolder.GetFiles("*.png", SearchOption.AllDirectories);
            LogAsset("Loading PNGs", TexturesTag);
            foreach (var file in PNGs)
            {
                var name = file.Name;
                if (!textures.ContainsKey(name))
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(file.FullName);
                        var tex = new Texture2D(0, 0);
                        tex.LoadImage(bytes);
                        tex.Apply();
                    }
                    catch (Exception e)
                    {
                        LogAsset(string.Format("Error while loading \"{0}\" :\n{1}", Path.Combine(file.Directory.Name, name), e), TexturesTag);
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

        public IEnumerator LoadAllAudioClips()
        {
            var MP3s = BiomesFolder.GetFiles("*.mp3", SearchOption.AllDirectories);
            LogAsset("Loading MP3s", AudioTag);
            yield return LoadAudioClipsFromFiles(MP3s, AudioType.MPEG);

            var WAVs = BiomesFolder.GetFiles("*.wav", SearchOption.AllDirectories);
            LogAsset("Loading WAVs", AudioTag);
            yield return LoadAudioClipsFromFiles(WAVs, AudioType.WAV);

            yield break;
        }

        IEnumerator LoadAudioClipsFromFiles(FileInfo[] files, AudioType type)
        {
            foreach (var file in files)
            {
                var name = file.Name;
                if (!sounds.ContainsKey(name))
                {
                    var path = Path.Combine("file://", file.FullName);
                    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, type))
                    {
                        yield return www.SendWebRequest();

                        try
                        {
                            if (www.isNetworkError)
                            {
                                LogAsset(string.Format("Error while loading \"{0}\" :\n{1}", Path.Combine(file.Directory.Name, name), www.error), AudioTag);
                            }
                            else
                            {
                                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                                sounds.Add(name, clip);
                            }
                        }
                        catch (Exception e)
                        {
                            LogAsset(string.Format("Error while loading \"{0}\" :\n{1}", Path.Combine(file.Directory.Name, name), e), AudioTag);
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

        static void LogAsset(string value, string tag = "")
        {
            Console.WriteLine(string.Format("[Nuterra.Biomes{0}] {1}", tag, value));
        }
    }
}
