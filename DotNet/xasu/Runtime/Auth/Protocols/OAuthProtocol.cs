using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xasu.Auth.Utils;
using System.Threading;
using Xasu.Util;
using Xasu.Auth.Protocols.OAuth;
using Xasu.Requests;
using TinCan;
using Xasu.Exceptions;
using System.Text;

namespace Xasu.Auth.Protocols
{
    public class OAuthProtocol : IAuthProtocol
    {
        protected readonly string fieldMissingMessage = "Field \"{0}\" required for \"OAuth 1.0a\" authentication is missing!";
        protected readonly string requestNullMessage = "Param \"headerParams\" required for \"OAuth 1.0a\" authentication is null!";

        // Standard fields
        protected readonly string consumerKeyField = "oauth_consumer_key";
        protected readonly string consumerSecretField = "oauth_consumer_secret";
        protected readonly string signatureMethodField = "oauth_signature_method";
        //protected readonly string signatureField = "oauth_signature";
        //protected readonly string timestampField = "oauth_timestamp";
        //protected readonly string callbackField = "oauth_callback";

        // Custom fields
        protected readonly string requestTokenEndpointField = "request_token_endpoint"; // AKA "initiate" endpoint
        protected readonly string authorizeEndpointField = "authorize_endpoint";
        protected readonly string accessTokenEndpointField = "access_token_endpoint";
        protected readonly string homePageField = "homepage";

        // Bearer
        protected string consumerKey;
        protected string consumerSecret;
        protected SignatureTypes signatureMethod;
        protected string requestTokenEndpoint;
        protected string authorizeEndpoint;
        protected string accessTokenEndpoint;
        protected OAuthAuthorization token;

        public IAsyncPolicy Policy { get; set; }

        public IHttpRequestHandler RequestHandler { get; set; }

        public Agent Agent { get; protected set; }

        public AuthState State { get; protected set; }

        public string ErrorMessage { get; protected set; }

        public virtual async Task Init(IDictionary<string, string> config)
        {
            // Main params
            consumerKey = config.GetRequiredValue(consumerKeyField, fieldMissingMessage);
            consumerSecret = config.GetRequiredValue(consumerSecretField, fieldMissingMessage);
            var signatureName = config.GetRequiredValue(signatureMethodField, fieldMissingMessage).ToUpperInvariant();

            switch (signatureName)
            {
                case "HMAC-SHA1": signatureMethod = SignatureTypes.HMACSHA1; break;
                case "RSA-SHA1": signatureMethod = SignatureTypes.RSASHA1; break;
                case "PLAINTEXT": signatureMethod = SignatureTypes.PLAINTEXT; break;
                default: throw new NotSupportedException("Method \"" + signatureName + "\" not supported, please use HMAC-SHA1, RSA-SHA1, or PLAINTEXT");
            }

            // Endpoints
            requestTokenEndpoint = config.Value(requestTokenEndpointField);
            authorizeEndpoint = config.Value(authorizeEndpointField);
            accessTokenEndpoint = config.Value(accessTokenEndpointField);

            var port = RandomHelper.Next(25525, 65535);
            var cancelationToken = new CancellationToken();

            try
            {
                // Prepare recepcion
                var oauthListener = new OAuthListener();
                string callbackUrl = AuthUtility.ListenForCallback(port, oauthListener, cancelationToken);

                // Get Temporary Token and check if our callback is ok
                var temporaryToken = await DoTokenRequest(requestTokenEndpoint, consumerKey, callbackUrl);
                if (!temporaryToken.OAuthCallbackConfirmed)
                {
                    throw new Exception("Callback not confirmed!");
                }

                // Get authorize token
                var authorizeResponse = await DoAuthorizeRequest(authorizeEndpoint, temporaryToken, oauthListener);
                var doAccessTokenRequest = await DoAccessTokenRequest(accessTokenEndpoint, consumerKey, authorizeResponse);

                var homePage = authorizeEndpoint.Replace((new Uri(authorizeEndpoint)).AbsolutePath, "");
                if (config.ContainsKey(homePageField))
                {
                    homePage = config.Value(homePageField);
                }
                Agent = new Agent
                {
                    account = new AgentAccount
                    {
                        homePage = homePage,
                        name = doAccessTokenRequest.OAuthToken
                    }
                };
            }
            catch (NetworkException nex)
            {
                State = AuthState.Errored;
                ErrorMessage = "Network is missing! " + nex.ToString();
            }
            catch (APIException apiEx)
            {
                State = AuthState.Errored;
                ErrorMessage = "Auhtorization failed with API exception! " + apiEx.ToString();
            }
        }

        protected virtual async Task<TemporaryAuthorization> DoTokenRequest(string requestTokenEndpoint, string consumerKey, string callbackUrl)
        {
            var request = new HttpRequest { url = requestTokenEndpoint, method = "POST" };
            request.policy = Policy;
            request.form = new Dictionary<string, string>()
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_callback", callbackUrl }       // We listen for the code
            };

            var response = await RequestHandler.SendRequest(request);
            return DeserializeFromResponse<TemporaryAuthorization>(response);
        }

        protected virtual async Task<AuthorizeResponse> DoAuthorizeRequest(string authorizeEndpoint, TemporaryAuthorization tempAuth, OAuthListener listener)
        {
            var url = RequestHandler.AppendParamsToExistingQueryString(authorizeEndpoint, new Dictionary<string, string>()
            {
                { "oauth_token", tempAuth.OAuthToken }
            });

            AuthorizeResponse authorizeResponse = null;
            listener.onAuthorizeResponse += (auth) =>
            {
                authorizeResponse = auth;
            };
            ApplicationSettings.OpenURL(url);

            while (authorizeResponse == null)
            {
                await Task.Yield();
            }
            return authorizeResponse;
        }

        protected virtual async Task<OAuthAuthorization> DoAccessTokenRequest(string accessTokenEndpoint, string consumerKey, AuthorizeResponse authorizeResponse)
        {
            var request = new HttpRequest { url = accessTokenEndpoint, method = "POST" };
            request.policy = Policy;
            request.form = new Dictionary<string, string>()
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_token", authorizeResponse.OAuthToken },
                { "oauth_verifier", authorizeResponse.OAuthVerifier }
            };

            var response = await RequestHandler.SendRequest(request);
            return DeserializeFromResponse<OAuthAuthorization>(response);
        }

        protected static T DeserializeFromResponse<T>(HttpResponse response)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(response.content));
        }

        public virtual Task UpdateParamsForAuth(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(requestNullMessage);
            }

            OAuthBase oAuthBase = new OAuthBase();
            var timestamp = oAuthBase.GenerateTimeStamp();
            var nonce = oAuthBase.GenerateNonce();
            var signature = oAuthBase.GenerateSignature(new Uri(request.url), consumerKey,
                                                        consumerSecret, token.OAuthToken, token.OAuthTokenSecret, request.method, timestamp, nonce, signatureMethod,
                                                        out string normalizedUrl,
                                                        out string normalizedRequestParameters);

            // Here we use the Query authorization instead of the Header authorization
            // More info: https://datatracker.ietf.org/doc/html/rfc5849#section-3.5.3
            request.url = string.Format("{0}?{1}&oauth_signature={2}", normalizedUrl, normalizedRequestParameters,
                                          signature);

            // #1
            return Task.FromResult(0);
        }

        public virtual void Unauthorized(APIException apiException)
        {
            State = AuthState.RequiresInteraction;
            ErrorMessage = "The authorization is invalid or has expired. Please Log in again!";
        }

        public virtual void Forbidden(APIException apiException)
        {
            State = AuthState.RequiresInteraction;
            ErrorMessage = "The current authorization has insufficient permissions for one required action. Please Log in again!";
        }
    }
}
