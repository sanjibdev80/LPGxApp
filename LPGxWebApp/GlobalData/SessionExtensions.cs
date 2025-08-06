using Newtonsoft.Json;

namespace LPGxWebApp.GlobalData
{
    public static class SessionExtensions
    {
        // Serialize and store object in session
        public static void SetObject(this ISession session, string key, object value)
        {
            var serializedObject = JsonConvert.SerializeObject(value); // JSON serialization
            session.SetString(key, serializedObject);
        }

        // Retrieve the object from session
        public static T GetObject<T>(this ISession session, string key)
        {
            var serializedObject = session.GetString(key);
            return serializedObject == null ? default : JsonConvert.DeserializeObject<T>(serializedObject);
        }

    }
}
