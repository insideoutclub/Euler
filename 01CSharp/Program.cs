using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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

        // Generate [1, 1000)
        // Filter out multiples of 3 and 5
        // Returns sum
        public static int filterImperative()
        {
            var sum = 0;
            for (var i = 1; i < 1000; ++i)
                if (i % 3 == 0 || i % 5 == 0)
                    sum += i;
            return sum;
        }

        // Sum multiples of 3
        // Sum multiples of 5
        // Subtract multiples of 3 * 5 to eliminate duplicates
        public static int generateMultiplesImperative()
        {
            var sum = 0;
            for (var i = 3; i < 1000; i += 3) sum += i;
            for (var i = 5; i < 1000; i += 5) sum += i;
            for (var i = 15; i < 1000; i += 15) sum -= i;
            return sum;
        }

        // Compute the sum of [x, 1000) step x using triangular numbers
        public static int sumOfMultiplesOf(int x)
        {
            var p = (1000 - 1) / x;
            return x * p * (p + 1) / 2;
        }

        // Sum multiples of 3
        // Sum multiples of 5
        // Subtract multiples of 3 * 5 to eliminate duplicates
        public static int sumOfMultiples()
        {
            return sumOfMultiplesOf(3) + sumOfMultiplesOf(5) - sumOfMultiplesOf(15);
        }

        // Generate [1, 1000)
        // Filter out multiples of 3 and 5
        // Returns sum
        public static int filterFunctional()
        {
            return Enumerable.Range(1, 1000 - 1).Where(i => i % 3 == 0 || i % 5 == 0).Sum();
        }

        // Generate multiples of x up to but not including 1000
        public static IEnumerable<int> multiplesOf(int x)
        {
            for (var i = x; i < 1000; i += x) yield return i;
        }

        // Sum multiples of 3
        // Sum multiples of 5
        // Subtract multiples of 3 * 5 to eliminate duplicates
        public static int generateMultiplesFunctional()
        {
            return multiplesOf(3).Sum() + multiplesOf(5).Sum() - multiplesOf(3 * 5).Sum();
        }

        static void Main(string[] args)
        {
            var experiments = new Func<int>[] {
                filterImperative,
                generateMultiplesImperative,
                sumOfMultiples,
                filterFunctional,
                generateMultiplesFunctional,
            };

            foreach (var result in experiments
                .Select(func => new { result = func(), time = benchmark(() => func(), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
                Console.WriteLine("{0,-30} {1} {2:00.0000}", result.name, result.result, result.time);
        }
    }
}
