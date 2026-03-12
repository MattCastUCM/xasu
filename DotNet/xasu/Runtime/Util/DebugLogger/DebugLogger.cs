using System;

namespace Xasu.Util
{
    public class DebugLogger : Delegate<IDebugLogger, BaseDebugLogger>
    {
        static DebugLogger()
        {
            InitInstance(Factories.Id.DEBUG_LOGGER);
        }

        public static void LogError(object message) => _instance.LogError(message);
        public static void LogException(Exception exception) => _instance.LogException(exception);
        public static void LogWarning(object message) => _instance.LogWarning(message);
        public static void Log(object message) => _instance.Log(message);
    }
}
