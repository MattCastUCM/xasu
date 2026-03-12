namespace Xasu.Util
{
    public interface IPersistentPreferences
    {
        public void Save();
        public void DeleteAll();
        public bool HasKey(string key);
        public void DeleteKey(string key);

        public void SetFloat(string key, float value);
        public float GetFloat(string key);
        public void SetInt(string key, int value);
        public int GetInt(string key);
        public void SetString(string key, string value);
        public string GetString(string key);
    }
}
