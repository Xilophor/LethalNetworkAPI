using Newtonsoft.Json;

namespace LethalNetworkAPI;

internal class JsonParser
{
    internal static string Parse(object data)
    {
        return JsonConvert.SerializeObject(data);
    }

    internal static object Parse(string json)
    {
        return JsonConvert.DeserializeObject(json);
    }
}