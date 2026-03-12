using System;

namespace Xasu.Util
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static readonly Lazy<T> _lazyInstance = new Lazy<T>(() => new T());
        protected Singleton() { }

        public static T Instance => _lazyInstance.Value;
    }
}
