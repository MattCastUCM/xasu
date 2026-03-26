using Assets.XasuUnity.Util;
using System.Threading.Tasks;
using UnityEngine;
using Xasu;
using Xasu.Auth;
using Xasu.CMI5;
using Xasu.Config;
using Xasu.Requests;
using Xasu.Util;

public class XasuUnityComponent : MonoBehaviour
{
    [SerializeField]
    float processingLoopTime = 1;   // In Seconds
    [SerializeField]
    bool autoStart = false,
         enableDebugLogging = false,
         canLoadConfigFromURL = false,
         sendRequestsInBackground = false;

    Task initTask = null;
    public Task InitTask
    {
        get { return initTask; }
    }

    /// <summary>
    /// The instance.
    /// </summary>
    static XasuUnityComponent instance;

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static XasuUnityComponent Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<XasuUnityComponent>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(XasuUnityComponent).Name;
                    instance = obj.AddComponent<XasuUnityComponent>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        // Change every factory method for their Unity wrapper 
        Factories.factories[Factories.Id.DEBUG_LOGGER] = () =>
        {
            return new UnityDebugLogger();
        };
        Factories.factories[Factories.Id.APPLICATION_SETTINGS] = () =>
        {
            return new UnityApplicationSettings();
        };
        Factories.factories[Factories.Id.PERSISTENT_PREFS] = () =>
        {
            return new UnityPersistentPrefs();
        };

        Factories.factories[Factories.Id.XASU_TRACKER] = () =>
        {
            return UnityTracker.Instance;
        };
        Factories.factories[Factories.Id.REQUEST_HANDLER] = () =>
        {
            return new UnityRequestHandler(sendRequestsInBackground);
        };
        Factories.factories[Factories.Id.TRACKER_CONFIG] = () =>
        {
            return new UnityTrackerConfig();
        };

        Factories.factories[Factories.Id.PKCE] = () =>
        {
            return new UnityPCKE();
        };
        Factories.factories[Factories.Id.AUTH_FACTORY] = () =>
        {
            return new UnityAuthFactory();
        };
        Factories.factories[Factories.Id.AUTH_UTILITY] = () =>
        {
            return new UnityAuthUtility();
        };
        Factories.factories[Factories.Id.CMI5_UTILITY] = () =>
        {
            return new UnityCmi5Utility();
        };

        XasuTracker.ProcessingLoopTime = processingLoopTime;
        XasuTracker.EnableDebugLogging = enableDebugLogging;
        XasuTracker.CanLoadConfigFromURL = canLoadConfigFromURL;


        if (autoStart)
        {
            initTask = XasuTracker.Init();
            await initTask;
        }
    }
}