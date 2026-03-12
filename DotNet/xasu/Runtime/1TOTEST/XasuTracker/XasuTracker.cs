using System;
using System.Threading.Tasks;
using TinCan;
using Xasu.Auth.Protocols;
using Xasu.Config;
using Xasu.Requests;
using Xasu.Util;

namespace Xasu
{
    public class XasuTracker : Delegate<ITracker, BaseTracker>
    {
        static XasuTracker()
        {
            InitInstance(Factories.Id.XASU_TRACKER);
        }

        // Properties
        public static TrackerStatus Status => _instance.Status;
        public static TrackerConfig TrackerConfig => _instance.TrackerConfig;
        public static Guid DefaultContextRegistrationId
        {
            get => _instance.DefaultContextRegistrationId;
            set => _instance.DefaultContextRegistrationId = value;
        }
        public static Context DefaultContext
        {
            get => _instance.DefaultContext;
            set => _instance.DefaultContext = value;
        }
        public static string DefaultIdPrefix
        {
            get => _instance.DefaultIdPrefix;
            set => _instance.DefaultIdPrefix = value;
        }
        public static float ProcessingLoopTime
        {
            get => _instance.ProcessingLoopTime;
            set => _instance.ProcessingLoopTime = value;
        }
        public static bool EnableDebugLogging 
        {
            get => _instance.EnableDebugLogging;
            set => _instance.EnableDebugLogging = value;
        }
        public static bool CanLoadConfigFromURL
        {
            get => _instance.CanLoadConfigFromURL;
            set => _instance.CanLoadConfigFromURL = value;
        }

        // Initialization
        public static Task InitOffline(string user, string mail) => _instance.InitOffline(user, mail);
        public static Task InitOfflineWithAccount(string user, string homePg) => _instance.InitOfflineWithAccount(user, homePg);
        public static Task Init(TrackerConfig trackerConfig = null, IHttpRequestHandler requestHandler = null, IAuthProtocol onlineAuthorization = null, IAuthProtocol backupAuthorization = null)
            => _instance.Init(trackerConfig, requestHandler, onlineAuthorization, backupAuthorization);

        // Tracker lifespan
        public static Task<Statement> Enqueue(Statement statement) => _instance.Enqueue(statement);
        public static Task Flush() => _instance.Flush();
        public static Task ResetState() => _instance.ResetState();
        public static Task Finalize(IProgress<float> progress = null) => _instance.Finalize(progress);

        // Logging
        public static void Log(string message) => _instance.Log(message);
        public static void LogError(string error, Exception ex = null) => _instance.LogError(error, ex);
        public static void LogWarning(string warning) => _instance.LogWarning(warning);
    }
}
