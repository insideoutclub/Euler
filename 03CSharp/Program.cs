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

        static IEnumerable<int> divisors()
        {
            yield return 2;
            yield return 3;
            var result = 5;
            yield return result;
            for (var addend = 2; true; addend = addend == 2 ? 4 : 2)
                yield return result += addend;
        }

        struct QuotientAndRemainder
        {
            public long quotient;
            public long remainder;
        }

        static QuotientAndRemainder quotientAndRemainder(long dividend, long divisor)
        {
            QuotientAndRemainder result;
            result.quotient = Math.DivRem(dividend, divisor, out result.remainder);
            return result;
        }

        static long largestPrimeFactor(long n)
        {
            var divisor = 2;
            var result = 1;
            while(n > 1)
            {
                var q = quotientAndRemainder(n, divisor);
                if(q.remainder == 0)
                {
                    result = divisor;
                    n = q.quotient;
                    while((q = quotientAndRemainder(n, divisor)).remainder == 0)
                        n = q.quotient;
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
                var q = quotientAndRemainder(n, divisor);
                if (q.remainder == 0)
                {
                    result = divisor;
                    n = q.quotient;
                    while ((q = quotientAndRemainder(n, divisor)).remainder == 0)
                        n = q.quotient;
                }
                divisor += divisor == 2 ? 1 : 2;
            }
            return result;
        }

        static long largestPrimeFactor3(long n)
        {
            var divisor = 2;
            var result = 1;
            while (n > 1)
            {
                var q = new QuotientAndRemainder();
                while((q = quotientAndRemainder(n, divisor)).remainder == 0)
                {
                    result = divisor;
                    n = q.quotient;
                }
                divisor += divisor == 2 ? 1 : 2;
            }
            return result;
        }

        static void Main(string[] args)
        {
            var experiments = new Function[] {
                largestPrimeFactor,
                largestPrimeFactor2,
                largestPrimeFactor3,
            };

            const long n = 600851475143;

            foreach (var result in experiments
                .Select(func => new { result = func(n), time = Benchmark(() => func(n), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
                Console.WriteLine("{0,-30} {1} {2:0.0000}", result.name, result.result, result.time);
        }
    }
}
