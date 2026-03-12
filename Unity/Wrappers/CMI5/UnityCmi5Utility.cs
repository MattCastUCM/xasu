using System;
using Xasu.Util;
using UnityEngine;
#if UNITY_WEBGL
using UnityEngine.Networking;
#endif

namespace Xasu.CMI5
{
    internal class UnityCmi5Utility : BaseCmi5Utility
    {
        public override string GetParam(string name)
        {
#if UNITY_WEBGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return UnityWebRequest.UnEscapeURL(WebGLUtility.GetParameter(name));
            }
#endif

            if (ApplicationSettings.Platform == (int)BaseApplicationSettings.SupportedPlatforms.WINDOWS && Application.isEditor)
            {
                var uri = new Uri(GameObject.FindObjectOfType<ArgSimulator>().cmi5Arg);
#if NET_4_6
                    var queryDictionary = UriHelper.DecodeQueryParameters(uri.Query);
#else
                var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
#endif
                return queryDictionary.Get(name);
            }
            else
            {
                base.GetParam(name);
            }
            throw new NotImplementedException("Cmi5 not implemented in other platforms yet!");
        }
    }
}
