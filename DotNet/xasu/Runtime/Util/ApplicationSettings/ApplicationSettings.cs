namespace Xasu.Util
{
    public class ApplicationSettings : Delegate<IApplicationSettings, BaseApplicationSettings>
    {
        static ApplicationSettings()
        {
            InitInstance(Factories.Id.APPLICATION_SETTINGS);
        }

        public static int Platform => _instance.Platform;
        public static string ProductName => _instance.ProductName;
        public static string CompanyName => _instance.CompanyName;
        public static string TrackerConfigPath => _instance.TrackerConfigPath;
        public static string AssetsPath => _instance.AssetsPath;
        public static string PersistentDataPath => _instance.PersistentDataPath;
        public static string TemporaryCachePath => _instance.TemporaryCachePath;

        public static void OpenURL(string url) => _instance.OpenURL(url);
        public static bool IsDesktopPlatform() => _instance.IsDesktopPlatform();
    }
}
