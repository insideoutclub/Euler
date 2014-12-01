using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Function = System.Func<long, long>;

namespace _03CSharp
{
    internal static class Program
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

        private static IEnumerable<long> Divisors()
        {
            for (var result = 2; true; ++result)
                yield return result;
        }

        private static IEnumerable<TResult> Unfold<T, TResult>(this T next, Func<T, Tuple<TResult, T>> generator)
        {
            var result = Tuple.Create(default(TResult), next);
            while ((result = generator(result.Item2)) != null)
                yield return result.Item1;
        }

        private struct Generator
        {
            private readonly long _p;

            public Generator(long p) { _p = p; }

            public Tuple<long, long> Generate(long n)
            {
                if (n % _p != 0)
                    return null;
                var next = n / _p;
                return Tuple.Create(next, next);
            }
        }

        private static IEnumerable<long> FactorsFound(long n, long p)
        {
            return n.Unfold(new Generator(p).Generate);
        }

        private static IEnumerable<long> PrimeFactors2(long n)
        {
#if false
            foreach (var p in Divisors())
            {
                if (p * p > n)
                    break;
                foreach (var x in FactorsFound(n, p))
                {
                    n = x;
                    yield return p;
                }
            }
            if (n > 1)
                yield return n;
#else
            var firstDivisor = Divisors().FirstOrDefault(divisor => n % divisor == 0);
            return null;
#endif
        }

        private static bool isFactor(long n, long d)
        {
            return n % d == 0;
        }

        private static long nextDivisor(long d)
        {
            return d == 2 ? 3 : d + 2;
        }

        private static long nextFactor(long n, long d)
        {
            var x = nextDivisor(d);
            return isFactor(n, x) ? x : nextFactor(n, x);
        }

        private static List<long> findFactors(long n, long d, List<long> acc)
        {
            if (isFactor(n, d))
            {
                acc.Add(d);
                return findFactors(n / d, d, acc);
            }
            else if (n > d)
            {
                return findFactors(n, nextFactor(n, d), acc);
            }
            return acc;
        }

        private static IEnumerable<long> PrimeFactors3(long n)
        {
            return findFactors(n, 2, new List<long>());
        }

        static IEnumerable<long> divisors()
        {
            yield return 2;
            var result = 3L;
            while (true)
            {
                yield return result;
                result += 2;
            }
        }

        static IEnumerable<long> factor(long n, IEnumerator<long> divisors)
        {
            var p = divisors.Current;
            if (p * p > n)
            {
                yield return n;
            }
            else if (n % p == 0)
            {
                yield return p;
                foreach (var item in factor(n / p, divisors))
                    yield return item;
            }
            else
            {
                divisors.MoveNext();
                foreach (var item in factor(n, divisors))
                    yield return item;
            }
        }

        private static IEnumerable<long> PrimeFactors(long n)
        {
            var d = divisors().GetEnumerator();
            d.MoveNext();
            return factor(n, d);
        }

        private static long largestPrimeFactor(long n)
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

        private static long largestPrimeFactor2(long n)
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

        private static long largestPrimeFactor3(long n)
        {
            var divisor = 2;
            var result = 1;
            while (divisor * divisor <= n)
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

        private static long largestPrimeFactor4(long n)
        {
            var divisor = 2;
            var result = 1;
            var addend = 2;
            while (divisor * divisor <= n)
            {
                while (n % divisor == 0)
                {
                    result = divisor;
                    n /= divisor;
                }
                switch(divisor) 
                {
                    case 2:
                        divisor = 3;
                        break;
                    case 3:
                        divisor = 5;
                        break;
                    default:
                        divisor += addend;
                        addend = addend == 2 ? 4 : 2;
                        break;
                }
            }
            return n == 1 ? result : n;
        }

        private static long largestPrimeFactor5(long n)
        {
            var divisor = 2;
            var result = 1;
            var gaps = new[] { 4, 2, 4, 2, 4, 6, 2, 6 };
            var i = 0;
            while (divisor * divisor <= n)
            {
                while (n % divisor == 0)
                {
                    result = divisor;
                    n /= divisor;
                }
                switch (divisor)
                {
                    case 2:
                        divisor = 3;
                        break;
                    case 3:
                        divisor = 5;
                        break;
                    case 5:
                        divisor = 7;
                        break;
                    default:
                        divisor += gaps[i];
                        i = (i + 1) % gaps.Length;
                        break;
                }
            }
            return n == 1 ? result : n;
        }

        private static void Main(string[] args)
        {
            var experiments = new Function[]
            {
                largestPrimeFactor,
                largestPrimeFactor2,
                largestPrimeFactor3,
                largestPrimeFactor4,
                largestPrimeFactor5,
            };
            const long n = 600851475143;
            Console.WriteLine(string.Join(", ", PrimeFactors(n)));
            return;

            foreach (var result in experiments
                .Select(func => new { result = func(n), time = Benchmark(() => func(n), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
                Console.WriteLine("{0,-30} {1} {2:0.0000}", result.name, result.result, result.time);
        }
    }
}
