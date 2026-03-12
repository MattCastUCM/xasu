using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xasu.Util;

namespace Xasu.Config
{
    public class TrackerConfig
    {
        protected const string DEFAULT_TRACKER_CONFIG_FILE_NAME = "tracker_config.json";
        protected string _filePath = "";
        protected string _fileName = "";
        protected string _fullPath = "";

        // Main Tracker Settings
        [JsonProperty("strict_mode")]
        public virtual bool StrictMode { get; set; }
        [JsonProperty("flush_interval")]
        public virtual float FlushInterval { get; set; } = 1f;
        [JsonProperty("simva")]
        public virtual bool Simva { get; set; }
        [JsonProperty("token")]
        public virtual bool Token { get; set; }

        // LRS Settings (Online)
        [JsonProperty("online")]
        public virtual bool Online { get; set; }
        [JsonProperty("batch_size")]
        public virtual int BatchSize { get; set; } = 32;
        [JsonProperty("homepage")]
        public virtual string HomePage { get; set; }
        [JsonProperty("lrs_endpoint")]
        public virtual string LRSEndpoint { get; set; }
        [JsonProperty("fallback")]
        public virtual bool Fallback { get; set; }

        // Auth Settings
        [JsonProperty("auth_protocol")]
        public virtual string AuthProtocol { get; set; }
        [JsonProperty("auth_parameters")]
        public virtual IDictionary<string, string> AuthParameters { get; set; }

        // Local Settings
        [JsonProperty("offline")]
        public virtual bool Offline { get; set; }
        [JsonProperty("trace_format")]
        public virtual TraceFormats TraceFormat { get; set; }
        [JsonProperty("file_name")]
        public virtual string FileName { get; set; } = "traces.log";


        // Backup Settings
        [JsonProperty("backup")]
        public virtual bool Backup { get; set; }
        [JsonProperty("backup_file_name")]
        public virtual string BackupFileName { get; set; } = "backup.log";
        [JsonProperty("backup_trace_format")]
        public virtual TraceFormats BackupTraceFormat { get; set; }
        [JsonProperty("backup_endpoint")]
        public virtual string BackupEndpoint { get; set; }
        [JsonProperty("backup_request_config")]
        public virtual JObject BackupRequestConfig { get; set; }
        // Auth Settings
        [JsonProperty("backup_auth_protocol")]
        public virtual string BackupAuthProtocol { get; set; }
        [JsonProperty("backup_auth_parameters")]
        public virtual IDictionary<string, string> BackupAuthParameters { get; set; }


        public TrackerConfig(string fileName = null, string filePath = null)
        {
            LRSEndpoint = "https://localhost:443/";
            AuthParameters = new Dictionary<string, string>();
            BackupAuthParameters = new Dictionary<string, string>();

            if (filePath == null)
            {
                filePath = ApplicationSettings.TrackerConfigPath;
            }
            if (fileName == null)
            {
                fileName = DEFAULT_TRACKER_CONFIG_FILE_NAME;
            }
            _fileName = fileName;
            _filePath = filePath;
            _fullPath = Path.Combine(_filePath, _fileName);
        }

        protected virtual async Task<string> ReadConfigFile()
        {
            XasuTracker.Log($"[TRACKER CONFIG] Loading tracker config from {_fullPath}");
            if (!File.Exists(_fullPath))
            {
                throw new FileNotFoundException(_fullPath);
            }

            using (FileStream stream = new FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public virtual async Task LoadConfig()
        {
            XasuTracker.Log("[TRACKER CONFIG] Loading...");
            string contents = await ReadConfigFile();

            if (!string.IsNullOrEmpty(contents))
            {
                XasuTracker.Log("[TRACKER CONFIG] tracker_config.json content: " + contents);
                var config = JsonConvert.DeserializeObject<TrackerConfig>(contents);
                SetProperties(config);
            }
        }

        protected void SetProperties(TrackerConfig other)
        {
            // Get the properties values using reflection and replace them with the read values
            var properties = typeof(TrackerConfig).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                prop.SetValue(this, prop.GetValue(other));
            }
        }
    }
}
