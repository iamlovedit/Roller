using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Roller.Infrastructure.Utils;

public static class JsonExtensions
{
    static JsonExtensions()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            DateTimeZoneHandling = DateTimeZoneHandling.Local,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }

    public static string Serialize(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static T? Deserialize<T>(this string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}