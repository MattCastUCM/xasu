using Polly;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xasu.Auth.Protocols;
using Xasu.Requests;
using Xasu.Util;

namespace Xasu.Auth
{
    /// <summary>
    /// Auth Manager manages the available authorization protocols and their initialization and continuation.
    /// </summary>
    public class AuthFactory : Delegate<IAuthFactory, BaseAuthFactory>
    {
        static AuthFactory()
        {
            InitInstance(Factories.Id.AUTH_FACTORY);
        }

        public static async Task<IAuthProtocol> InitAuth(string authName, IDictionary<string, string> parameters, IHttpRequestHandler requestHandler = null, IAsyncPolicy policy = null)
        {
            return await _instance.InitAuth(authName, parameters, requestHandler, policy);
        }
    }
}
