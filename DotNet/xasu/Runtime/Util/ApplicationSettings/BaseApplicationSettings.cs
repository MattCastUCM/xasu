using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Xasu.Util
{
    /// <summary>
    /// Default class for the ApplicationSettings delegate
    /// </summary>
    public class BaseApplicationSettings : IApplicationSettings
    {
        public enum SupportedPlatforms { NONE = -1, WINDOWS, MACOS, LINUX, LAST };

        protected static int _platform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return (int)SupportedPlatforms.WINDOWS;
                }
                // TODO: Test if platform is correctly detected for macOS
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return (int)SupportedPlatforms.MACOS;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return (int)SupportedPlatforms.LINUX;
                }
                return (int)SupportedPlatforms.NONE;
            }
        }
        public virtual int Platform => _platform;

        const string DEFAULT_APP_NAME = "MyProject";
        const string DEFAULT_COMP_NAME = "DefaultCompany";

        protected string _productName = "";
        /// <summary>
        /// Application name
        /// </summary>
        public virtual string ProductName
        {
            get { return _productName; }
            protected set { _productName = value; }
        }

        protected string _companyName = "";
        /// <summary>
        /// Company/studio/developer name
        /// </summary>
        public virtual string CompanyName
        {
            get { return _companyName; }
            protected set { _companyName = value; }
        }


        #region PathSettings
        const string DEFAULT_TRACKER_CONFIG_PATH = "./StreamingAssets";
        const string DEFAULT_ASSETS_PATH = "./Assets";

        protected string _trackerConfigPath = "";
        /// <summary>
        /// Path of the folder where the tracker config file will be stored in
        /// </summary
        public virtual string TrackerConfigPath
        {
            get { return _trackerConfigPath; }
            protected set { _trackerConfigPath = value; }
        }

        protected string _assetsPath = "";
        /// <summary>
        /// Path of the assets folder
        /// </summary>
        public virtual string AssetsPath
        {
            get { return _assetsPath; }
            protected set { _assetsPath = value; }
        }


        protected string _persistentDataPath = "";
        /// <summary>
        /// Directory to store data that must be retained between executions
        /// The files can only be erased by users directly and not by any app updates
        /// 
        /// Windows: %userprofile%/AppData/Roaming/Xasu/(CompanyName)/(ProductName)
        /// Linux: ~/.config/Xasu/(CompanyName)/(ProductName)                            (via XDG standards)
        /// macOS: ~/Library/Application Support/Xasu/(CompanyName)/(ProductName)
        /// </summary>
        public virtual string PersistentDataPath
        {
            get { return _persistentDataPath; }
            protected set { _persistentDataPath = value; }
        }


        protected string _temporaryCachePath = "";
        /// <summary>
        /// Path to a temporary data/cache directory 
        /// 
        /// Windows: %userprofile%/AppData/Local/XasuCache/(CompanyName)/(ProductName)
        /// Linux: ~/.local/share/XasuCache/(CompanyName)/(ProductName)
        /// macOS: ~/Library/Application Support/XasuCache/(CompanyName)/(ProductName)/cache    OR   ~/Library/Caches/XasuCache/(CompanyName)/(ProductName)
        /// </summary>
        public virtual string TemporaryCachePath
        {
            get { return _temporaryCachePath; }
            protected set { _temporaryCachePath = value; }
        }

        #endregion


        public BaseApplicationSettings() : this(DEFAULT_APP_NAME, DEFAULT_COMP_NAME, DEFAULT_TRACKER_CONFIG_PATH, DEFAULT_ASSETS_PATH) { }

        /// <summary>
        /// IF THE PATHS FORMATTING ARE PLATFORM DEPENDENT, THE CONSTRUCTOR SHOULD BE CALLED WITH
        /// DIFFERENT PARAMETERS FOR EACH SUPPORTED PLATFORM IN ORDER TO MATCH THEIR FILE SYSTEMS
        /// </summary>
        /// <param name="prodName">Application name</param>
        /// <param name="compName">Company/Studio/Developer name</param>
        /// <param name="trckConfPath">Directory where the tracker config file will be stored in</param>
        /// <param name="assetPath">Directory where the assets will be stored in</param>
        /// <param name="persDataPath">Directory where data will be persisted in</param>
        /// <param name="cachePath">Directory where cache will be stored in</param>
        public BaseApplicationSettings(string prodName = DEFAULT_APP_NAME, string compName = DEFAULT_COMP_NAME,
            string trckConfPath = DEFAULT_TRACKER_CONFIG_PATH, string assetPath = DEFAULT_ASSETS_PATH, string persDataPath = null, string cachePath = null)
        {
            if (string.IsNullOrEmpty(prodName))
            {
                prodName = DEFAULT_APP_NAME;
            }
            ProductName = prodName;

            if (string.IsNullOrEmpty(compName))
            {
                compName = DEFAULT_COMP_NAME;
            }
            CompanyName = compName;

            if (string.IsNullOrEmpty(trckConfPath))
            {
                trckConfPath = DEFAULT_TRACKER_CONFIG_PATH;
            }
            TrackerConfigPath = trckConfPath;

            if (string.IsNullOrEmpty(assetPath))
            {
                assetPath = DEFAULT_ASSETS_PATH;
            }
            AssetsPath = assetPath;

            if (string.IsNullOrEmpty(persDataPath))
            {
                persDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xasu", compName, _productName);
            }
            PersistentDataPath = persDataPath;
            Directory.CreateDirectory(PersistentDataPath);

            if (string.IsNullOrEmpty(cachePath))
            {
                cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XasuCache", compName, _productName);
            }
            TemporaryCachePath = cachePath;
            Directory.CreateDirectory(TemporaryCachePath);
        }

        protected virtual string FixUrlFormat(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return url = "https://" + url;
            }
            return url;
        }
        public virtual void OpenURL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            url = FixUrlFormat(url);

            try
            {
                switch (Platform)
                {
                    case (int)SupportedPlatforms.WINDOWS:
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                        break;

                    case (int)SupportedPlatforms.MACOS:
                        Process.Start("open", url);
                        break;

                    case (int)SupportedPlatforms.LINUX:
                        Process.Start("xdg-open", url);
                        break;
                }
            }
            catch
            {
                DebugLogger.LogWarning($"Couldn't open url: {url}");
            }
        }

        public virtual bool IsDesktopPlatform()
        {
            return Platform > (int)SupportedPlatforms.NONE && Platform < (int)SupportedPlatforms.LAST;
        }
    }
}
