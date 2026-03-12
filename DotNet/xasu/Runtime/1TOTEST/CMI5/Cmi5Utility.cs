using Xasu.Util;

namespace Xasu.CMI5
{
    public class Cmi5Utility : Delegate<ICmi5Utility, BaseCmi5Utility>
    {
        static Cmi5Utility()
        {
            InitInstance(Factories.Id.CMI5_UTILITY);
        }

        public static string GetParam(string name)
        {
            return _instance.GetParam(name);
        }
    }
}
