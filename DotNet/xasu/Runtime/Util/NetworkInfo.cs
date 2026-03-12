namespace Xasu.Util
{
    public static class NetworkInfo
    {
        private static bool isWorking = true;

        public static bool IsWorking()
        {
            return isWorking;
        }

        public static void Failed()
        {
            isWorking = false;
        }

        public static void Worked()
        {
            isWorking = true;
        }
    }
}
