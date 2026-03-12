using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Xasu.Util
{
    /// <summary>
    /// Default class for the persistent preferences delegate
    /// </summary>
    public class BasePersistentPrefs : IPersistentPreferences
    {
        protected const string DEFAULT_FILE_NAME = "preferences.json";
        protected string _filePath = "";
        protected string _fileName = DEFAULT_FILE_NAME;
        protected string _fullPath = "";

        protected Dictionary<string, object> _values = new Dictionary<string, object>();


        public BasePersistentPrefs() : this(null, null) { }
        public BasePersistentPrefs(string fileName = null, string filePath = null)
        {
            // Execute Save() when closing the program or cancelling execution
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Save();
            Console.CancelKeyPress += (s, e) => Save();

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = ApplicationSettings.PersistentDataPath;
            }
            _filePath = filePath;

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = DEFAULT_FILE_NAME;
            }
            _fileName = fileName;

            _fullPath = Path.Combine(_filePath, _fileName);
            _fullPath = Path.GetFullPath(_fullPath);

            Load();
        }

        /// <summary>
        /// Load preferences from the file
        /// </summary>
        protected virtual void Load()
        {
            try
            {
                using (StreamReader file = File.OpenText(_fullPath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject jsonObject = JObject.Load(reader);
                    foreach (var item in jsonObject)
                    {
                        _values.Add(item.Key, item.Value);
                    }
                }
            }
            catch
            {
                DebugLogger.LogWarning($"{_fullPath} doesn't exist or can't be opened. Settings won't be loaded");
            }
        }

        /// <summary>
        /// Stores the dictionary key-values in a json file
        /// It is recommended to not call this function during gameplay, as it creates a new file and saves all the key-values each time 
        /// </summary>
        public virtual void Save()
        {
            try
            {
                // Create the directory if it doesn't exist
                string directory = Path.GetDirectoryName(_fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // always creates a new file (overwriting the old one if it exists) so deleted keys get removed as well
                using (FileStream fs = File.Create(_fullPath))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonTextWriter jw = new JsonTextWriter(sw))
                {
                    DebugLogger.Log($"Saving preferences in {_fullPath}");

                    jw.Formatting = Formatting.Indented;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, _values);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogException(ex);
            }
        }

        /// <summary>
        /// Deletes all the stored key-values from both the dictionary and file
        /// </summary>
        public virtual void DeleteAll()
        {
            _values.Clear();
            Save();
        }
        /// <summary>
        /// Deletes the key from both the dictionary and json file
        /// </summary>
        public virtual void DeleteKey(string key)
        {
            if(_values.Remove(key))
            {
                Save();
            }
        }

        public virtual bool HasKey(string key)
        {
            return _values.ContainsKey(key);
        }
        
        protected virtual void SetValue(string key, object value)
        {
            _values[key] = value;
        }

        public virtual void SetFloat(string key, float value)
        {
            SetValue(key, value);
        }
        public virtual float GetFloat(string key)
        {
            if (HasKey(key) && _values[key] is float)
            {
                return (float)_values[key];
            }
            return 0;
        }

        public virtual void SetInt(string key, int value)
        {
            SetValue(key, value);
        }
        public virtual int GetInt(string key)
        {
            if (HasKey(key) && _values[key] is int)
            {
                return (int)_values[key];
            }
            return 0;
        }

        public virtual void SetString(string key, string value)
        {
            SetValue(key, value);
        }
        public virtual string GetString(string key)
        {
            if (HasKey(key) && _values[key] is string)
            {
                return (string)_values[key];
            }
            return string.Empty;
        }
    }

}
