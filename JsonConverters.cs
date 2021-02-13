using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        public override bool CanRead => true;
    }

    public class ScriptableObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ScriptableObject).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var res = ScriptableObject.CreateInstance(objectType);

            var resJSON = JObject.ReadFrom(reader);

            var fields = objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                try
                {
                    if (resJSON[field.Name] != null)
                    {
                        field.SetValue(res, resJSON[field.Name].ToObject(field.FieldType, serializer));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(field.Name);
                    Console.WriteLine(e);
                }
            }

            return res;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite => false;
        public override bool CanRead => true;
    }

    public class ScriptableObjectCreationConverter : CustomCreationConverter<ScriptableObject>
    {
        public override bool CanConvert(Type objectType)
        {
            Console.WriteLine(objectType.Name);
            return typeof(ScriptableObject).IsAssignableFrom(objectType);
        }

        public override ScriptableObject Create(Type objectType)
        {
            return ScriptableObject.CreateInstance(objectType);
        }
    }

    public class ArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(T).MakeArrayType().IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JArray.FromObject(reader.Value).ToObject(typeof(T).MakeArrayType());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite => false;
        public override bool CanRead => true;
    }

    public class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Color) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JObject.ReadFrom(reader).ToObject<Color>(JsonSerializer.CreateDefault(new JsonSerializerSettings() {
                MissingMemberHandling = MissingMemberHandling.Ignore
            }));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var obj = new JObject();
            var color = (Color)value;
            obj.Add("r", color.r);
            obj.Add("g", color.g);
            obj.Add("b", color.b);
            obj.Add("a", color.a);

            obj.WriteTo(writer);
        }

        public override bool CanWrite => true;
        public override bool CanRead => true;
    }
}
