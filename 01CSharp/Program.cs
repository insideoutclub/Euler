using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Function = System.Func<int, int, int, int>;

namespace _01CSharp
{
    class Program
    {
        // Run action the number of times specified by iterations
        // Returns the elapsed time in milliseconds
        public static double benchmark(Action action, int iterations)
        {
            action();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i != iterations; ++i)
                action();
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        // Generate [1, limit)
        // Filter out multiples of x and y
        // Returns sum
        public static int filterImperative(int limit, int x, int y)
        {
            var sum = 0;
            for (var i = 1; i < limit; ++i)
                if (i % x == 0 || i % y == 0)
                    sum += i;
            return sum;
        }

        // Sum multiples of x
        // Sum multiples of y
        // Subtract multiples of x * y to eliminate duplicates
        public static int generateMultiplesImperative(int limit, int x, int y)
        {
            var sum = 0;
            for (var i = x; i < limit; i += x) sum += i;
            for (var i = y; i < limit; i += y) sum += i;

            var product = x * y;
            for (var i = product; i < limit; i += product) sum -= i;
            return sum;
        }

        // Compute the sum of [x, limit) step x using triangular numbers
        public static int sumOfMultiplesOf(int x, int limit)
        {
            var p = (limit - 1) / x;
            return x * p * (p + 1) / 2;
        }

        // Sum multiples of x
        // Sum multiples of y
        // Subtract multiples of x * y to eliminate duplicates
        public static int sumOfMultiples(int limit, int x, int y)
        {
            return sumOfMultiplesOf(x, limit) + sumOfMultiplesOf(y, limit) - sumOfMultiplesOf(x * y, limit);
        }

        // Generate [1, limit)
        // Filter out multiples of x and y
        // Returns sum
        public static int filterFunctional(int limit, int x, int y)
        {
            return Enumerable.Range(1, limit - 1).Where(i => i % x == 0 || i % y == 0).Sum();
        }

        // Generate multiples of x up to but not including limit
        public static IEnumerable<int> multiplesOf(int x, int limit)
        {
            for (var i = x; i < limit; i += x) yield return i;
        }

        // Sum multiples of x
        // Sum multiples of y
        // Subtract multiples of x * y to eliminate duplicates
        public static int generateMultiplesFunctional(int limit, int x, int y)
        {
            return multiplesOf(x, limit).Sum() + multiplesOf(y, limit).Sum() - multiplesOf(x * y, limit).Sum();
        }

        static void Main(string[] args)
        {
            var experiments = new Function[] {
                filterImperative,
                generateMultiplesImperative,
                sumOfMultiples,
                filterFunctional,
                generateMultiplesFunctional,
            };

            const int limit = 1000, x = 3, y = 5;
            foreach (var result in experiments
                .Select(func => new { result = func(limit, x, y), time = benchmark(() => func(limit, x, y), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
                Console.WriteLine("{0,-30} {1} {2:00.0000}", result.name, result.result, result.time);
        }
    }
}
