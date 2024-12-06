using Newtonsoft.Json;

namespace GreenGardenClient.Models
{
    public static class SessionExtensions
    {
        // Method to store an object as a JSON string in the session
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            // Serialize the object to JSON and store it in the session
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        // Method to retrieve an object from JSON string in the session
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            // Get the JSON string from the session
            var value = session.GetString(key);
            // Deserialize the JSON string back to the object
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
