using Newtonsoft.Json;

namespace LethalNetworkAPI;

internal static class JsonParser
{
    internal static string Parse(object data)
    {
        if (data is string s)
            return s;
        
        return "json|"+JsonConvert.SerializeObject(data);
    }

    internal static object Parse(string json)
    {
        return json.StartsWith("json|") ? JsonConvert.DeserializeObject(json.Remove(0,5)) : json;
    }
}