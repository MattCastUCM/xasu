namespace Xasu.Util
{
    public interface IApplicationSettings
    {
        public int Platform { get; }
        public string ProductName { get; }
        public string CompanyName { get; }
        public string TrackerConfigPath { get; }
        public string AssetsPath { get; }
        public string PersistentDataPath { get; }
        public string TemporaryCachePath { get; }

        public void OpenURL(string url);
        public bool IsDesktopPlatform();
    }
}