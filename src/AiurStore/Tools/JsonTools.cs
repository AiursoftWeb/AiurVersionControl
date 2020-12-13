using System.Text.Json;

namespace AiurStore.Tools
{
    public static class JsonTools
    {
        public static string Serialize<T>(T model)
        {
           return JsonSerializer.Serialize(model, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
