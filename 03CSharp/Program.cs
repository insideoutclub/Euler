using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Function = System.Func<long, long>;

namespace _03CSharp
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

        static long largestPrimeFactor(long n)
        {
            var divisor = 2;
            var result = 1;
            while(n > 1)
            {
                while(n % divisor == 0)
                {
                    result = divisor;
                    n /= divisor;
                }
                ++divisor;
            }
            return result;
        }

        static long largestPrimeFactor2(long n)
        {
            var divisor = 2;
            var result = 1;
            while (n > 1)
            {
                while(n % divisor == 0)
                {
                    result = divisor;
                    n /= divisor;
                }
                divisor += divisor == 2 ? 1 : 2;
            }
            return result;
        }

        static long largestPrimeFactor3(long n)
        {
            var divisor = 2;
            var result = 1;
            while (n > 1 && divisor * divisor <= n)
            {
                while (n % divisor == 0)
                {
                    result = divisor;
                    n /= divisor;
                }
                divisor += divisor == 2 ? 1 : 2;
            }
            return n == 1 ? result : n;
        }

        static long largestPrimeFactor4(long n)
        {
            var divisor = 2;
            var result = 1;
            var addend = 2;
            while (n > 1 && divisor * divisor <= n)
            {
                while (n % divisor == 0)
                {
                    result = divisor;
                    n /= divisor;
                }
                switch(divisor) 
                {
                    case 2: divisor = 3; break;
                    case 3: divisor = 5; break;
                    default: divisor += addend; addend = addend == 2 ? 4 : 2; break;
                }
            }
            return n == 1 ? result : n;
        }


        static void Main(string[] args)
        {
            var experiments = new Function[] {
                largestPrimeFactor,
                largestPrimeFactor2,
                largestPrimeFactor3,
                largestPrimeFactor4,
            };

            const long n = 600851475143;
            foreach (var result in experiments
                .Select(func => new { result = func(n), time = Benchmark(() => func(n), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
                Console.WriteLine("{0,-30} {1} {2:0.0000}", result.name, result.result, result.time);
        }
    }
}
