using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Function = System.Func<int>;

namespace _02CSharp
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

        static IEnumerable<int> fibonacci()
        {
            var a = 1;
            var b = 2;
            while (true)
            {
                yield return a;
                var sum = a + b;
                a = b;
                b = sum;
            }
        }

        static int filterImperative()
        {
            int a = 1, b = 2, total = 0;
            while (a <= 4000000)
            {
                if ((a & 1) == 0) total += a;
                var sum = a + b;
                a = b;
                b = sum;
            }
            return total;
        }

        static int noFilterImperative()
        {
            var a = 2;
            var b = 8;
            var total = 0;
            while (a <= 4000000)
            {
                total += a;
                var sum = a + 4 * b;
                a = b;
                b = sum;
            }
            return total;
        }

        static bool isEven(int x) { return (x & 1) == 0; }

        static int filterFunctional()
        {
            return fibonacci().Where(isEven).TakeWhile(x => x <= 4000000).Sum();
        }

        static IEnumerable<int> evenFibonacci()
        {
            var a = 2;
            var b = 8;
            while(true)
            {
                yield return a;
                var sum = 4 * b + a;
                a = b;
                b = sum;
            }
        }

        static int noFilterFunctional()
        {
            return evenFibonacci().TakeWhile(x => x <= 4000000).Sum();
        }

        static void Main(string[] args)
        {
            var experiments = new Function[] {
                filterImperative,
                filterFunctional,
                noFilterFunctional,
                noFilterImperative,
            };

            foreach (var result in experiments
                .Select(func => new { result = func(), time = benchmark(() => func(), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
                Console.WriteLine("{0,-30} {1} {2:0.0000}", result.name, result.result, result.time);
        }
    }
}
