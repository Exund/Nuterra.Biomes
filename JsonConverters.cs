using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Nuterra.Biomes.JsonConverters
{
    public class UnityObjectConverter<T> : JsonConverter where T : UnityEngine.Object
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var val = reader.Value.ToString();
            var res = Resources.GetObjectFromUserResources(objectType, val);

            if(!res)
            {
                res = Resources.GetObjectFromGameResources(objectType, val);
            }

            return res;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite => false;
    }
}
