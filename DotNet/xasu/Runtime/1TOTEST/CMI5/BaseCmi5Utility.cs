using System;
using Xasu.Util;

namespace Xasu.CMI5
{
    public class BaseCmi5Utility : ICmi5Utility
    {
        public virtual string GetParam(string name)
        {
            if (ApplicationSettings.Platform == (int)BaseApplicationSettings.SupportedPlatforms.WINDOWS)
            {
                var cliArgs = Environment.GetCommandLineArgs();
                var cmi5ParamIndex = Array.IndexOf(cliArgs, "-cmi5");
                if (cmi5ParamIndex == -1 || cmi5ParamIndex >= cliArgs.Length)
                {
                    throw new NotImplementedException("Cmi5 param not found or wrong formatted!");
                }
                var cmi5Args = cliArgs[cmi5ParamIndex + 1];

                var uri = new Uri(cmi5Args);
#if NET_4_6
                var queryDictionary = UriHelper.DecodeQueryParameters(uri.Query);
#else
                var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
#endif
                return queryDictionary.Get(name);
            }
            throw new NotImplementedException("Cmi5 not implemented in other platforms yet!");
        }
    }
}
