using System.Threading.Tasks;

namespace Xasu.Util
{
    internal static class ExtensionsTask
    {
        public static async void WrapErrors(this Task task)
        {
            await task;
        }
    }
}
