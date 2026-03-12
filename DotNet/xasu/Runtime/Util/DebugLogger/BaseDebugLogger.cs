using System;

namespace Xasu.Util
{
    /// <summary>
    /// Default class for the DebugLogger delegate
    /// </summary>
    public class BaseDebugLogger : IDebugLogger
    {
        public virtual void LogError(object message)
        {
#if DEBUG
            string msg = "[ERR]: " + message.ToString();
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }
#endif
        }
        public virtual void LogException(Exception exception)
        {
#if DEBUG
            string msg = "[EXCEPT]: " + exception.ToString();
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }
#endif
        }
        public virtual void LogWarning(object message)
        {
#if DEBUG
            string msg = "[WARN]: " + message.ToString();
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }
#endif
        }
        public virtual void Log(object message)
        {
#if DEBUG
            string msg = "[LOG]: " + message.ToString();
            try
            {
                Console.ResetColor();
                Console.WriteLine(msg);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }
#endif
        }
    }
}
