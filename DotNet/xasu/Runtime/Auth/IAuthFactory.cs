using Polly;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xasu.Auth.Protocols;
using Xasu.Requests;

namespace Xasu.Auth
{
    public interface IAuthFactory
    {
        public Task<IAuthProtocol> InitAuth(string authName, IDictionary<string, string> parameters, IHttpRequestHandler requestHandler = null, IAsyncPolicy policy = null);
    }
}
