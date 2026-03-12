using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Xasu.Requests;
using Xasu.Util;
using static Xasu.Util.UnityApplicationSettings;

namespace Xasu.Config
{
    internal class UnityTrackerConfig : TrackerConfig
    {
        protected override async Task<string> ReadConfigFile()
        {
            if (ApplicationSettings.IsDesktopPlatform())
            {
                return await base.ReadConfigFile();
            }
            else
            {
                UnityWebRequest reader = null;
                string contents = null;

                // Platform dependent StreamingAssets Load https://docs.unity3d.com/Manual/StreamingAssets.html
                switch (ApplicationSettings.Platform)
                {
                    case (int)UnityApplicationSettings.SupportedUnityPlatforms.WEBGL:
                        reader = UnityWebRequest.Get(Path.Combine(ApplicationSettings.TrackerConfigPath, _fileName));
                        break;
                    case (int)UnityApplicationSettings.SupportedUnityPlatforms.ANDROID:
                        reader = UnityWebRequest.Get("jar:file://" + ApplicationSettings.AssetsPath + "!/assets/" + _fileName);
                        break;
                    case (int)UnityApplicationSettings.SupportedUnityPlatforms.IOS:
                        _fullPath = Path.Combine(ApplicationSettings.AssetsPath, "Raw", _fileName);
                        break;
                }

                if (reader != null)
                {
                    XasuTracker.Log($"[TRACKER CONFIG] Requesting tracker_config.json from url: {reader.uri}");
                    await UnityRequestHandler.DoRequest(reader);
                    contents = reader.downloadHandler.text;
                }
                else
                {
                    return await base.ReadConfigFile();
                }

                return contents;
            }
        }

        public override async Task LoadConfig()
        {
            if (ApplicationSettings.Platform == (int)SupportedUnityPlatforms.WEBGL && XasuTracker.CanLoadConfigFromURL)
            {
                var trackerConfig = WebGLUtility.GetUrlTrackerConfig();
                if (trackerConfig != null)
                {
                    XasuTracker.Log("[TRACKER CONFIG] Loaded tracker_config from URL parameters.");
                    base.SetProperties(trackerConfig);
                }
            }
            else
            {
                await base.LoadConfig();
            }
        }
    }
}
