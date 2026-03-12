using System;

namespace Xasu.Util
{
    public interface IDebugLogger
    {
        public void LogError(object message);
        public void LogException(Exception exception);
        public void LogWarning(object message);
        public void Log(object message);
    }
}
