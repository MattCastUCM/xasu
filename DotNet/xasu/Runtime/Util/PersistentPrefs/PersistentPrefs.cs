namespace Xasu.Util
{
    public class PersistentPrefs : Delegate<IPersistentPreferences, BasePersistentPrefs>
    {
        static PersistentPrefs()
        {
            InitInstance(Factories.Id.PERSISTENT_PREFS);
        }

        public static void Save() => _instance.Save();
        public static void DeleteAll() => _instance.DeleteAll();
        public static bool HasKey(string key) => _instance.HasKey(key);
        public static void DeleteKey(string key) => _instance.DeleteKey(key);

        public static void SetFloat(string key, float value) => _instance.SetFloat(key, value);
        public static float GetFloat(string key) => _instance.GetFloat(key);
        public static void SetInt(string key, int value) => _instance.SetInt(key, value);
        public static int GetInt(string key) => _instance.GetInt(key);
        public static void SetString(string key, string value) => _instance.SetString(key, value);
        public static string GetString(string key) => _instance.GetString(key);
    }
}
