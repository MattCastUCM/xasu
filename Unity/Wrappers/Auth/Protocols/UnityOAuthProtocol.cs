using System.Collections.Generic;
using System.Threading.Tasks;
using Xasu.Auth.Protocols.OAuth;
using Xasu.Requests;
using Xasu.Util;

namespace Xasu.Auth.Protocols
{
    public class UnityOAuthProtocol : OAuthProtocol
    {
        protected override async Task<TemporaryAuthorization> DoTokenRequest(string requestTokenEndpoint, string consumerKey, string callbackUrl)
        {
            var request = new HttpRequest { url = requestTokenEndpoint, method = "POST" };
            request.policy = Policy;
            request.form = new Dictionary<string, string>()
            {
                { "oauth_consumer_key", consumerKey },
#if UNITY_WEBGL && !UNITY_EDITOR
                { "oauth_callback", WebGLUtility.GetUrl() }, // Returns to itself
#else
                { "oauth_callback", callbackUrl }       // We listen for the code
#endif
            };

            var response = await RequestHandler.SendRequest(request);
            return DeserializeFromResponse<TemporaryAuthorization>(response);
        }

        protected override async Task<AuthorizeResponse> DoAuthorizeRequest(string authorizeEndpoint, TemporaryAuthorization tempAuth, OAuthListener listener)
        {
            var url = RequestHandler.AppendParamsToExistingQueryString(authorizeEndpoint, new Dictionary<string, string>()
            {
                { "oauth_token", tempAuth.OAuthToken }
            });

#if !UNITY_WEBGL || UNITY_EDITOR
            AuthorizeResponse authorizeResponse = null;
            listener.onAuthorizeResponse += (auth) =>
            {
                authorizeResponse = auth;
            };
#endif
            ApplicationSettings.OpenURL(url);

#if !UNITY_WEBGL || UNITY_EDITOR
            while (authorizeResponse == null)
            {
                await Task.Yield();
            }
            return authorizeResponse;
#else
            return null;
#endif
        }
    }
}
