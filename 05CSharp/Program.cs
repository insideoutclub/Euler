using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Function = System.Func<int>;

namespace _05CSharp
{
    class Program
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

        private static int gcd(int a, int b)
        {
            while(true)
            {
                if (b == 0) return a;
                a = a % b;
                if (a == 0) return b;
                b = b % a;
            }
        }

        private static int steingcd(int m, int n)
        {
            if (m == 0) return n;
            if (n == 0) return m;
            var d = 0;
            while((m & 1) == 0 && (n & 1) == 0)
            {
                m >>= 1;
                n >>= 1;
                ++d;
            }
            while ((m & 1) == 0)
                m >>= 1;
            while ((n & 1) == 0)
                n >>= 1;
            while (true)
                if (m < n)
                {
                    n = n - m;
                    do n >>= 1; while ((n & 1) == 0);
                }
                else if (n < m)
                {
                    m = m - n;
                    do m >>= 1; while ((m & 1) == 0);
                }
                else
                    return m << d;
        }

        private static int lcm(int a, int b)
        {
            return Math.Abs(a) / gcd(a, b) * Math.Abs(b);
        }

        private static int steinlcm(int a, int b)
        {
            return Math.Abs(a) / steingcd(a, b) * Math.Abs(b);
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

        private static int functional()
        {
            var result = 1;
            for (int i = 2; i != 20; ++i)
                result = steinlcm(result, i);
            return result;
        }

        static void Main(string[] args)
        {
            var experiments = new Function[]
            {
                imperative,
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
