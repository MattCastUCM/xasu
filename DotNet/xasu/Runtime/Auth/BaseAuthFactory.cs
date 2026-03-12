using Polly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xasu.Auth.Protocols;
using Xasu.Requests;

namespace Xasu.Auth
{
    public class BaseAuthFactory : IAuthFactory
    {
        protected const string NOT_SUPPORTED_AUTH_MESSAGE = "Authorization type \"{0}\" not supported. Accepted types: basic, oauth and oauth2.";

        protected Dictionary<string, IAuthProtocol> _authProtocols = new Dictionary<string, IAuthProtocol>()
        {
            { "basic", new BasicProtocol() },
            { "oauth", new OAuthProtocol() },
            { "oauth2", new OAuth2Protocol() },
            { "cmi5", new Cmi5Protocol() }
        };

        public virtual async Task<IAuthProtocol> InitAuth(string authName, IDictionary<string, string> parameters, IHttpRequestHandler requestHandler = null, IAsyncPolicy policy = null)
        {
            if (authName == null || authName == "none" || authName == "disabled")
            {
                return null;
            }

            if (!_authProtocols.ContainsKey(authName))
            {
                throw new NotSupportedException(string.Format(NOT_SUPPORTED_AUTH_MESSAGE, authName));
            }

            if (requestHandler != null)
            {
                _authProtocols[authName].RequestHandler = requestHandler;
            }

            if (policy != null)
            {
                _authProtocols[authName].Policy = policy;
            }

            await _authProtocols[authName].Init(parameters);

            return _authProtocols[authName];
        }
    }

}
