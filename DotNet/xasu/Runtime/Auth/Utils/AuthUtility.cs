using System.Collections.Generic;
using System.Threading;
using Xasu.Util;

namespace Xasu.Auth.Utils
{
    public class AuthUtility : Delegate<IAuthUtility, BaseAuthUtility>
    {
        static AuthUtility()
        {
            InitInstance(Factories.Id.AUTH_UTILITY);
        }

        public static string ListenForCallback(int port, IAuthListener authListener, CancellationToken cancelationToken)
        {
            return _instance.ListenForCallback(port, authListener, cancelationToken);
        }
        public static string Value(IDictionary<string, string> data, string key)
        {
            return _instance.Value(data, key);
        }

        public static string GetRequiredValue(IDictionary<string, string> data, string key, string missingMessage)
        {
            return _instance.GetRequiredValue(data, key, missingMessage);
        }
    }
}
