using System.Collections.Generic;
using System.Threading;

namespace Xasu.Auth.Utils
{
    public interface IAuthUtility
    {
        public string ListenForCallback(int port, IAuthListener authListener, CancellationToken cancelationToken);
        public string Value(IDictionary<string, string> data, string key);
        public string GetRequiredValue(IDictionary<string, string> data, string key, string missingMessage);
    }
}
