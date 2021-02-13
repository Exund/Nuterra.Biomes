using System;
using System.Collections;
using System.Collections.Generic;
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
            yield return StartCoroutine(Resources.LoadAllTextures());
            yield return StartCoroutine(Resources.LoadAllAudioClips());
            yield return StartCoroutine(Resources.LoadAllTerrainLayers());
            yield return StartCoroutine(Resources.LoadAllMapGenerators());
            yield return StartCoroutine(Resources.LoadAllBiomes());

            /*foreach (var item in Resources.userResources)
            {
                Console.WriteLine(item.Key.Name);
                foreach (var item2 in item.Value)
                {
                    Console.WriteLine(item2.Key + " " + JsonConvert.SerializeObject(item2.Value, new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
                        {
                            DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        },
                        Converters = {
                            new JsonConverters.ColorConverter()
                        }
                    }));
                    
                }
            }*/

            yield break;
        }
    }
}
