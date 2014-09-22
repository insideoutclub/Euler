using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace _01CSharp
{
    class Program
    {
        public static KeyValuePair<T, double> Benchmark<T>(Func<T> func, int iterations)
        {
            var result = func(); // Don't count the first run

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i != iterations; ++i)
                func();
            stopwatch.Stop();
            return new KeyValuePair<T, double>(result, stopwatch.Elapsed.TotalMilliseconds);
        }

        public bool IsMultipleOf(int x, int y) { return y % x == 0; }

        public static int imperative()
        {
            var sum = 0;
            for (var i = 1; i != 1000; ++i)
                if (i % 3 == 0 || i % 5 == 0)
                    sum += i;
            return sum;
        }

        public static int imperative2()
        {
            var sum = 0;
            for (var i = 3; i < 1000; i += 3)
                sum += i;
            for (var i = 5; i < 1000; i += 5)
                sum += i;
            for (var i = 15; i < 1000; i += 15)
                sum -= i;
            return sum;
        }

        public static int functional()
        {
            return Enumerable.Range(1, 999).Where(i => i % 3 == 0 || i % 5 == 0).Sum();
        }

        public static IEnumerable<int> MultiplesOf(int first, int last)
        {
            var step = first;
            for (; first < last; first += step)
                yield return first;
        }

        public static int functional2()
        {
            return MultiplesOf(3, 1000).Sum() + MultiplesOf(5, 1000).Sum() - MultiplesOf(15, 1000).Sum();
        }

        static void Main(string[] args)
        {
            var experiments = new[] {
                new KeyValuePair<string, Func<int>>("imperative", imperative),
                new KeyValuePair<string, Func<int>>("imperative2", imperative2),
                new KeyValuePair<string, Func<int>>("functional", functional),
                new KeyValuePair<string, Func<int>>("functional2", functional2),
            };
            Console.WriteLine(experiments[0].Value.Method.Name);
            foreach(var result in experiments.Select(kvp => new KeyValuePair<string, KeyValuePair<int, double>>(kvp.Key, Benchmark(kvp.Value, 1000))).OrderBy(kvp => kvp.Value.Value))
                Console.WriteLine(result);
        }
    }
}
