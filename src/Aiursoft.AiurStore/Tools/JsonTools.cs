using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aiursoft.AiurStore.Tools
{
    public static class JsonTools
    {
        private static JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static string Serialize<T>(T model)
        {
           return JsonConvert.SerializeObject(model, _settings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }
}
