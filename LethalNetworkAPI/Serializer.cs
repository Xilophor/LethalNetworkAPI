using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
     
using Object = UnityEngine.Object;

namespace LethalNetworkAPI;

/* 
 * Taken (and modified) from https://forum.unity.com/threads/jsonutility-serializes-floats-with-way-too-many-digits.541045/#post-5485749.
 * Thank you to ModLunar! https://forum.unity.com/members/modlunar.1182760/
 */
internal static class Serializer
{
    #region Static Section
    //Not expected to work with UnityEditor types as well, since it won't ignore types like Editor, EditorWindow, PropertyDrawer, etc. -- with their fields.
    private class UnityImitatingContractResolver : DefaultContractResolver {
        /// <summary>
        /// Any data types whose fields we don't want to serialize. When any of these types are encountered during serialization,
        /// all of their fields will be skipped.
        /// </summary>
        private static readonly Type[] IgnoreTypes = new Type[] {
        };
        private static bool IsIgnoredType(Type type) => Array.FindIndex(IgnoreTypes, (Type current) => current == type) >= 0;

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
            var allFields = new List<FieldInfo>();
            var unityObjType = typeof(Object);

            for (var t = type; t != null && !IsIgnoredType(t); t = t.BaseType)
            {
                var currentTypeFields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in currentTypeFields)
                {
                    if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null)
                        continue;
                    if (unityObjType.IsAssignableFrom(field.FieldType)) {
                        Debug.LogError("Failed to serialize a Unity object reference -- this is not supported by the " + GetType().Name
                            + ". Ignoring the property. (" + type.Name + "'s \"" + field.Name + "\" field)");
                        continue;
                    }
                    allFields.Add(field);
                }
            }
            //This sorts them based on the order they were actually written in the source code.
            //Beats me why Reflection wouldn't list them in that order to begin with, but whatever, this works
            allFields.Sort((a, b) => a.MetadataToken - b.MetadataToken);

            var properties = new List<JsonProperty>(allFields.Count);
            foreach (var property in from t in allFields let index = properties.FindIndex((JsonProperty current) => current.UnderlyingName == t.Name) where index < 0 select CreateProperty(t, memberSerialization))
            {
                property.Writable = true;
                property.Readable = true;
                properties.Add(property);
            }
            return properties;
        }
    }

    private class FloatConverter : JsonConverter<float> {
        private int _decimalPlaces;
        private string? _format;

        public int DecimalPlaces {
            get => _decimalPlaces;
            set {
                _decimalPlaces = Mathf.Clamp(value, 0, 8);
                _format = "F" + _decimalPlaces;
            }
        }

        public FloatConverter(int decimalPlaces) {
            DecimalPlaces = decimalPlaces;
        }

        public override void WriteJson(JsonWriter writer, float value, JsonSerializer serializer) {
            writer.WriteValue(float.Parse(value.ToString(_format)));
        }

        public override float ReadJson(JsonReader reader, Type objectType, float existingValue, bool hasExistingValue, JsonSerializer serializer) {
            //For some reason, reader.Value is giving back a double and casting to a float did not go so well, from object to float.
            //And I didn't want to hard code 2 consecutive casts, literally, like "(float) (double) reader.Value", so I'm glad this works:
            return Convert.ToSingle(reader.Value);
        }
    }
    #endregion

    private const bool PrettyPrint = false;

    private static readonly JsonSerializerSettings? Settings = new() {
        ContractResolver = new UnityImitatingContractResolver(),
        Converters = new JsonConverter[] {
            new FloatConverter(3)
        }

    };

    public static string Serialize<T>(T originalObj) {
        string text;
        var obj = new ValueWrapper<T>(originalObj);
        const Formatting formatting = PrettyPrint ? Formatting.Indented : Formatting.None;
        Settings!.Formatting = formatting;
        try {
            //For now, as I am unsure how I want to move forward with looping references, I'll just have it try first -- if it comes up, show an error
            Settings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
            text = JsonConvert.SerializeObject(obj, Settings);
        } catch (JsonSerializationException e) {
            //and then go to these statements and ignore any looping references. This way, it lets us know if it DOES come across looping references.
            //Which aren't supported as this code is written currently.
            Debug.LogException(e);
            Settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            text = JsonConvert.SerializeObject(obj, Settings);
        }
        return text;
    }

    public static T Deserialize<T>(string text) {
        return JsonConvert.DeserializeObject<ValueWrapper<T>>(text, Settings)!.var!;
    }
}