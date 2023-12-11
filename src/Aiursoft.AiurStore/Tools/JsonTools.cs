using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aiursoft.AiurStore.Tools
{
    public static class JsonTools
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static string Serialize<T>(T model)
        {
           return JsonConvert.SerializeObject(model, Settings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
    }
}
