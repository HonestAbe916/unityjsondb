using System;
using System.IO;
using FullSerializer;
using UnityEngine;

namespace HASS.JSON
{
    /// <summary>
    /// Uses jacobdufault's https://github.com/jacobdufault/fullserializer to Serialize
    /// and Deserialize the data
    /// </summary>
    public static class JsonSerialization
    {
        private static readonly fsSerializer _serializer = new fsSerializer();

        public static string Serialize(Type type, object value)
        {
            // serialize the data
            fsData data;
            _serializer.TrySerialize(type, value, out data).AssertSuccessWithoutWarnings();

            // emit the data via JSON
            return fsJsonPrinter.CompressedJson(data);
        }

        public static object Deserialize(Type type, string serializedState)
        {
            //if (type.IsSubclassOf())
            // step 1: parse the JSON data
            fsData data = fsJsonParser.Parse(serializedState);

            // step 2: deserialize the data
            object deserialized = null;
            _serializer.TryDeserialize(data, type, ref deserialized).AssertSuccess();

            return deserialized;
        }

        public static void WriteJSON<T>(T obj, string path)
        {
            string json = Serialize(obj.GetType(), obj);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(json);
                }
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}