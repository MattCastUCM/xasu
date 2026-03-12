using Xasu.Auth.Protocols;

namespace Xasu.Auth
{
    internal class UnityAuthFactory : BaseAuthFactory
    {
        public UnityAuthFactory() : base() 
        {
            if (_authProtocols.ContainsKey("oauth"))
            {
                _authProtocols["oauth"] = new UnityOAuthProtocol();
            }
        }
    }
}
