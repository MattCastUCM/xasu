using UnityEngine;
using Xasu.CMI5;

namespace Xasu.Util
{
    internal class UnityApplicationSettings : BaseApplicationSettings
    {
        public enum SupportedUnityPlatforms { ANDROID = SupportedPlatforms.LAST, WEBGL, IOS, LAST };

        protected static new int _platform
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                {
                    return (int)SupportedPlatforms.WINDOWS;
                }
                else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
                {
                    return (int)SupportedPlatforms.MACOS;
                }
                else if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor)
                {
                    return (int)SupportedPlatforms.LINUX;
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    return (int)SupportedUnityPlatforms.ANDROID;
                }
                else if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    return (int)SupportedUnityPlatforms.WEBGL;
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    return (int)SupportedUnityPlatforms.IOS;
                }
                return (int)SupportedPlatforms.NONE;
            }
        }
        public override int Platform => _platform;
        public override string ProductName => Application.productName;
        public override string CompanyName => Application.companyName;

        public override string TrackerConfigPath => Application.streamingAssetsPath;

        public override string AssetsPath => Application.dataPath;

        public override string PersistentDataPath => Application.persistentDataPath;

        public override string TemporaryCachePath => Application.temporaryCachePath;

        public UnityApplicationSettings() { }

        public override void OpenURL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            url = FixUrlFormat(url);

            try
            {
                if (Platform == (int)SupportedUnityPlatforms.WEBGL)
                {
                    WebGLUtility.OpenUrl(url);
                }
                else
                {
                    Application.OpenURL(url);
                }
            }
            catch
            {
                DebugLogger.LogWarning($"Couldn't open url: {url}");
            }


        }

        public override bool IsDesktopPlatform()
        {
            return base.IsDesktopPlatform() || Application.isEditor;
        }
    }
}