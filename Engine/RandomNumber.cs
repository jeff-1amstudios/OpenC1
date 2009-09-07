namespace OneamEngine
{
    using System;

    public class RandomNumber
    {
        private static Random myrnd;

        public static int Next()
        {
            return rnd.Next();
        }

        public static int Next(int maxValue)
        {
            return rnd.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            return rnd.Next(minValue, maxValue);
        }

        public static double NextDouble()
        {
            return rnd.NextDouble();
        }

        public static float NextFloat()
        {
            return (float)rnd.NextDouble();
        }

        public static Random rnd
        {
            get
            {
                if (myrnd == null)
                {
                    myrnd = new Random();
                }
                return myrnd;
            }
        }
    }
}

