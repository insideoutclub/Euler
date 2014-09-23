using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Function = System.Func<int, int, int, int>;

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

        static IEnumerable<int> Fibonacci(int a, int b, int limit)
        {
            while (a < limit)
            {
                yield return a;
                var sum = a + b;
                a = b;
                b = sum;
            }
        }

        static int FilterInlineImperative(int a, int b, int limit)
        {
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

        static int NoFilterImperative(int a, int b, int limit)
        {
            var total = 0;
            a = 2;
            b = 8;
            while (a < limit)
            {
                total += a;
                var sum = a + 4 * b;
                a = b;
                b = sum;
            }
            return total;
        }

        static int FilterInlineFunctional(int a, int b, int limit)
        {
            return Fibonacci(a, b, limit).Where(x => x % 2 == 0).Sum();
        }

        static bool IsEven(int x) { return x % 2 == 0; }

        static int FilterFunctionCallFunctional(int a, int b, int limit)
        {
            return Fibonacci(a, b, limit).Where(IsEven).Sum();
        }

        static IEnumerable<int> EvenFibonacci(int a, int b, int limit)
        {
            while(a < limit)
            {
                yield return a;
                var sum = 4 * b + a;
                a = b;
                b = sum;
            }
        }

        static int NoFilterFunctional(int a, int b, int limit)
        {
            return EvenFibonacci(2, 8, limit).Sum();
        }

        static void Main(string[] args)
        {
            var experiments = new Function[] {
                FilterInlineFunctional,
                FilterInlineImperative,
                FilterFunctionCallFunctional,
                NoFilterFunctional,
                NoFilterImperative,
            };

            const int a = 1, b = 2, limit = 4000001;
            foreach (var result in experiments
                .Select(func => new { result = func(a, b, limit), time = Benchmark(() => func(a, b, limit), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
                Console.WriteLine("{0,-30} {1} {2:00.000}", result.name, result.result, result.time);
        }
    }
}
