using System;
using System.Collections.Generic;
using Xasu.Auth;
using Xasu.Auth.Protocols.OAuth2;
using Xasu.Auth.Utils;
using Xasu.CMI5;
using Xasu.Config;

namespace Xasu.Util
{
    public static class Factories
    {
        public enum Id { NONE, APPLICATION_SETTINGS, PERSISTENT_PREFS, DEBUG_LOGGER, XASU_TRACKER, TRACKER_CONFIG, PKCE, AUTH_FACTORY, AUTH_UTILITY, CMI5_UTILITY }
        public static Dictionary<Id, Func<object>> factories = new Dictionary<Id, Func<object>>()
        {
            { Id.APPLICATION_SETTINGS, () => { return new BaseApplicationSettings(); } },
            { Id.PERSISTENT_PREFS, () => { return new BasePersistentPrefs(); } },
            { Id.DEBUG_LOGGER, () => { return new BaseDebugLogger(); } },

            // DON'T RETURN NEW FOR SINGLETONS
            { Id.XASU_TRACKER, () => { return BaseTracker.Instance; } },
            { Id.TRACKER_CONFIG, () => { return new TrackerConfig(); } },

            { Id.PKCE, () => { return new BasePKCE(); }},
            { Id.AUTH_FACTORY, () => { return new BaseAuthFactory(); } },

            { Id.AUTH_UTILITY, () => { return new BaseAuthUtility(); } },
            { Id.CMI5_UTILITY, () => { return new BaseCmi5Utility(); } },
        };
    }
}
