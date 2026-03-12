using System;
using UnityEngine;

namespace Xasu.Util
{
    internal class UnityDebugLogger : BaseDebugLogger
    {
        public override void LogError(object message) => Debug.LogError(message);
        public override void LogException(Exception exception) => Debug.LogException(exception);
        public override void LogWarning(object message) => Debug.LogWarning(message);
        public override void Log(object message) => Debug.Log(message);
    }
}
