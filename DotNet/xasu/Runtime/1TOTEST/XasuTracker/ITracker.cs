using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TinCan;
using Xasu.Auth.Protocols;
using Xasu.Config;
using Xasu.Requests;

namespace Xasu
{
    public interface ITracker
    {
        public TrackerStatus Status { get; }

        public TrackerConfig TrackerConfig { get; }
        public Guid DefaultContextRegistrationId { get; set; }
        public Context DefaultContext { get; set; }
        public string DefaultIdPrefix { get; set; }

        public float ProcessingLoopTime { get; set; }
        bool EnableDebugLogging { get; set; }
        bool CanLoadConfigFromURL { get; set; }

        Task InitOffline(string user, string mail);
        Task InitOfflineWithAccount(string user, string homePg);
        Task Init(TrackerConfig trackerConfig = null, IHttpRequestHandler requestHandler = null, IAuthProtocol onlineAuthorization = null, IAuthProtocol backupAuthorization = null);

        public Task<Statement> Enqueue(Statement statement);
        public Task Flush();
        public Task ResetState();
        public Task Finalize(IProgress<float> progress = null);

        public void LogError(string error, Exception ex = null);
        public void LogWarning(string warning);
        public void Log(string message);
    }

}
