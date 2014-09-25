using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Function = System.Func<int, int>;

namespace _02CSharp
{
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

        static IEnumerable<int> Fibonacci(int limit)
        {
            var a = 1;
            var b = 2;
            while (a < limit)
            {
                yield return a;
                var sum = a + b;
                a = b;
                b = sum;
            }
        }

        static int FilterImperative(int limit)
        {
            var a = 1;
            var b = 2;
            var total = 0;
            while (a < limit)
            {
                if (a % 2 == 0) total += a;
                var sum = a + b;
                a = b;
                b = sum;
            }
            return total;
        }

        static int NoFilterImperative(int limit)
        {
            var a = 2;
            var b = 8;
            var total = 0;
            while (a < limit)
            {
                total += a;
                var sum = a + 4 * b;
                a = b;
                b = sum;
            }
            return total;
        }

        static bool IsEven(int x) { return x % 2 == 0; }

        static int FilterFunctional(int limit)
        {
            return Fibonacci(limit).Where(IsEven).Sum();
        }

        static IEnumerable<int> EvenFibonacci(int limit)
        {
            var a = 2;
            var b = 8;
            while(a < limit)
            {
                yield return a;
                var sum = 4 * b + a;
                a = b;
                b = sum;
            }
        }

        static int NoFilterFunctional(int limit)
        {
            return EvenFibonacci(limit).Sum();
        }

        static void Main(string[] args)
        {
            var experiments = new Function[] {
                FilterImperative,
                FilterFunctional,
                NoFilterFunctional,
                NoFilterImperative,
            };

            const int limit = 4000001;
            foreach (var result in experiments
                .Select(func => new { result = func(limit), time = Benchmark(() => func(limit), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
                Console.WriteLine("{0,-30} {1} {2:0.0000}", result.name, result.result, result.time);
        }
    }
}
