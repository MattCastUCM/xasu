using System;

namespace Xasu.Util
{
    public static class RandomHelper
    {
        private static readonly Random random = new Random();

        public static int Next() => random.Next();

        public static int Next(int maxValue) => random.Next(maxValue);

        public static int Next(int minValue, int maxValue) => random.Next(minValue, maxValue);
    }
}