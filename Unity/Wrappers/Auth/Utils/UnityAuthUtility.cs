using System;
using System.Threading;
using Xasu.Auth.Utils;
using Xasu.Util;

namespace Assets.XasuUnity.Util
{
    internal class UnityAuthUtility : BaseAuthUtility
    {
        /// <summary>
        /// ListenForCallback is a workaround to receive OAuth and OAuth2 redirects.
        /// It behaves differently in different platforms:
        ///     - Editor: Will listen through HTTP server.
        ///     - Android: 
        ///     - IOs:
        /// Once the callback is received, the application will stop listening. 
        /// Cancellation token is only used to abort the listening. 
        /// </summary>
        public override string ListenForCallback(int port, IAuthListener authListener, CancellationToken cancelationToken)
        {
            string redirectUrl = base.ListenForCallback(port, authListener, cancelationToken);

            if (ApplicationSettings.Platform == (int)UnityApplicationSettings.SupportedUnityPlatforms.WEBGL)
            {
                if (WebGLUtility.IsWebGLListening())
                {
                    DebugLogger.Log("Getting WebGLListener response from url: " + WebGLUtility.GetCompleteUrl());
                    WebGLUtility.SetWebGLListening(false);
                    string queryString = new Uri(WebGLUtility.GetCompleteUrl()).Query;
#if NET_4_6
                    var queryDictionary = UriHelper.DecodeQueryParameters(queryString);
#else
                    var queryDictionary = System.Web.HttpUtility.ParseQueryString(queryString);
#endif
                    authListener.OnAuthReply(queryDictionary);
                }
                else
                {
                    WebGLUtility.SetWebGLListening(true);
                }

                redirectUrl = WebGLUtility.GetUrl();
            }
            /*else
            {
                throw new NotImplementedException(string.Format("Platform \"{0}\" is not yet implemented!", Application.platform));
            }*/

            return redirectUrl;
        }
    }
}
