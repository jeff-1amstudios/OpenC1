using System;

namespace OneamEngine
{
    public class RandomGenerator
    {
        private Random _random;

        public RandomGenerator()
        {
            _random = new Random();
        }

        public int Next()
        {
            return _random.Next();
        }

        public float Next(float minValue, float maxValue)
        {
            return (float)(minValue + (float)_random.NextDouble() * (maxValue - minValue));
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }
    }
}

