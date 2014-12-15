using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Function = System.Func<int>;

namespace _05CSharp
{
    static class Program
    {
        private static double Benchmark(Action action, int iterations)
        {
            action();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i != iterations; ++i)
            {
                action();
            }
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        public static IEnumerable<TResult> Unfold<T, TResult>(this T next, Func<T, Tuple<TResult, T>> generator)
        {
            for (var result = Tuple.Create(default(TResult), next); (result = generator(result.Item2)) != null; )
            {
                yield return result.Item1;
            }
        }

        private static int functionalgcd(int a, int b)
        {
            return new { a, b }.Unfold(state => state.b == 0 ? null : Tuple.Create(state.b, new { a = state.b, b = state.a % state.b })).Last();
        }

        private static int functionallcm(int a, int b)
        {
            return Math.Abs(a) / functionalgcd(a, b) * Math.Abs(b);
        }

        private static int functional()
        {
            return Enumerable.Range(1, 20).Aggregate(functionallcm);
        }

        private static int gcd(int a, int b)
        {
            while(b != 0)
            {
                var r = a % b;
                a = b;
                b = r;
            }
            return a;
        }

        private static int lcm(int a, int b)
        {
            return Math.Abs(a) / gcd(a, b) * Math.Abs(b);
        }

        private static int imperative()
        {
            var result = 1;
            for(int i = 2; i != 20; ++i)
            {
                result = lcm(result, i);
            }
            return result;
        }

        private static int brute()
        {
            for(var i = 1; true; ++i)
            {
                var j = 2;
                for (; j != 21; ++j)
                {
                    if (i % j != 0)
                        break;
                }
                if (j == 21)
                    return i;
            }
        }

        static void Main(string[] args)
        {
            var experiments = new Function[]
            {
                imperative,
                //brute,
                functional,
            };
            foreach (var result in experiments
                .Select(func => new { result = func(), time = Benchmark(() => func(), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
            {
                Console.WriteLine("{0,-30} {1} {2:0.0000}", result.name, result.result, result.time);
            }
        }
    }
}
