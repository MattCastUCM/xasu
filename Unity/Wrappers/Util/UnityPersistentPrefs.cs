using UnityEngine;

namespace Xasu.Util
{
    internal class UnityPersistentPrefs : IPersistentPreferences
    {
        public UnityPersistentPrefs() { }

        public void Save() => PlayerPrefs.Save();
        public void DeleteAll() => PlayerPrefs.DeleteAll();
        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
        
        public bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public float GetFloat(string key) => PlayerPrefs.GetFloat(key);
        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);

        public int GetInt(string key) => PlayerPrefs.GetInt(key);
        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        public string GetString(string key) => PlayerPrefs.GetString(key);
        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
    }
}
