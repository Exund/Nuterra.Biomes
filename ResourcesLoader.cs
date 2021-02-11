using System;
using System.Collections;
using System.Collections.Generic;
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

            foreach (var item in Resources.userResources)
            {
                Console.WriteLine(item.Key.Name);
                foreach (var item2 in item.Value)
                {
                    Console.WriteLine(item2.Key + " " + JsonConvert.SerializeObject(item2.Value, new JsonSerializerSettings()
                    {
                        MaxDepth = 2,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }));
                    
                }
            }

            yield break;
        }
    }
}
