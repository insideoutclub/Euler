using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Function = System.Func<int, int, int, int>;

namespace _01CSharp
{
    static class ExtensionMethods
    {
        // Returns true if x is a multiple of y
        public static bool IsMultipleOf(this int x, int y) { return x % y == 0; }
    }

    class Program
    {
        // Run action the number of times specified by iterations
        // Returns the elapsed time in milliseconds
        public static double Benchmark(Action action, int iterations)
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
        public static int FilterInlineImperative(int limit, int x, int y)
        {
            var sum = 0;
            for (var i = 1; i < limit; ++i)
                if (i % x == 0 || i % y == 0)
                    sum += i;
            return sum;
        }

        // Same as FilterInlineImperative, except multiples filter is a function call
        public static int FilterFunctionCallImperative(int limit, int x, int y)
        {
            var sum = 0;
            for (var i = 1; i < limit; ++i)
                if (i.IsMultipleOf(x) || i.IsMultipleOf(y))
                    sum += i;
            return sum;
        }

        // Sum multiples of x
        // Sum multiples of y
        // Subtract multiples of x * y to eliminate duplicates
        public static int GenerateMultiplesImperative(int limit, int x, int y)
        {
            var sum = 0;
            for (var i = x; i < limit; i += x) sum += i;
            for (var i = y; i < limit; i += y) sum += i;
            for (var i = x * y; i < limit; i += x * y) sum -= i;
            return sum;
        }

        public static int SumMultiples(int x, int limit)
        {
            var p = (limit - 1) / x;
            return x * p * (p + 1) / 2;
        }

        // Sum multiples of x
        // Sum multiples of y
        // Subtract multiples of x * y to eliminate duplicates
        public static int SumMultiples(int limit, int x, int y)
        {
            return SumMultiples(x, limit) + SumMultiples(y, limit) - SumMultiples(x * y, limit);
        }

        // Generate [1, limit)
        // Filter out multiples of x and y
        // Returns sum
        public static int FilterInlineFunctional(int limit, int x, int y)
        {
            return Enumerable.Range(1, limit - 1).Where(i => i % x == 0 || i % y == 0).Sum();
        }

        // Same as FilterInlineFunctional, except multiples filter is a function call
        public static int FilterFunctionCallFunctional(int limit, int x, int y)
        {
            return Enumerable.Range(1, limit - 1).Where(i => i.IsMultipleOf(x) || i.IsMultipleOf(y)).Sum();
        }

        // Generate multiples of x up to but not including limit
        public static IEnumerable<int> MultiplesOf(int x, int limit)
        {
            for (var i = x; i < limit; i += x) yield return i;
        }

        // Sum multiples of x
        // Sum multiples of y
        // Subtract multiples of x * y to eliminate duplicates
        public static int GenerateMultiplesFunctional(int limit, int x, int y)
        {
            return MultiplesOf(x, limit).Sum() + MultiplesOf(y, limit).Sum() - MultiplesOf(x * y, limit).Sum();
        }

        static void Main(string[] args)
        {
            var experiments = new Function[] {
                FilterInlineImperative,
                FilterFunctionCallImperative,
                GenerateMultiplesImperative,
                SumMultiples,
                FilterInlineFunctional,
                FilterFunctionCallFunctional,
                GenerateMultiplesFunctional,
            };

            const int limit = 1000, x = 3, y = 5;
            foreach (var result in experiments
                .Select(func => new { result = func(limit, x, y), time = Benchmark(() => func(limit, x, y), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
                Console.WriteLine("{0,-30} {1} {2:00.000}", result.name, result.result, result.time);
        }
    }
}
