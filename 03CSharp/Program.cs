// Copyright (C) 2014 Teradyne, Inc. All rights reserved.
//
// This document contains proprietary and confidential information of Teradyne,
// Inc. and is tendered subject to the condition that the informaipation (a) be
// retained in confidence and (b) not be used or incorporated in any product
// except with the express written consent of Teradyne, Inc.
//
// Revision History:

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Function = System.Func<long, long>;

namespace Teradyne._03CSharp
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Program
    {
        private const long TWO = 2;
        private const long THREE = 3;
        private const long FOUR = 4;
        private const long FIVE = 5;
        private const long SEVEN = 7;

        /// <summary>
        /// Run action the number of times specified by iterations
        /// </summary>
        /// <param name="action"></param>
        /// <param name="iterations"></param>
        /// <returns>Returns the elapsed time in milliseconds</returns>
        public static double Benchmark(Action action, int iterations)
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="next"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        private static IEnumerable<TResult> Unfold<T, TResult>(this T next, Func<T, Tuple<TResult, T>> generator)
        {
            var result = Tuple.Create(default(TResult), next);
            while ((result = generator(result.Item2)) != null)
            {
                yield return result.Item1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static bool IsFactor(long n, long d)
        {
            return n % d == 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static long NextDivisor(long d)
        {
            return d == TWO ? THREE : d + TWO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static long NextFactor(long n, long d)
        {
            var x = NextDivisor(d);
            return IsFactor(n, x) ? x : NextFactor(n, x);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="d"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
        private static List<long> FindFactors(long n, long d, List<long> acc)
        {
            if (IsFactor(n, d))
            {
                acc.Add(d);
                return FindFactors(n / d, d, acc);
            }
            else if (n > d)
            {
                return FindFactors(n, NextFactor(n, d), acc);
            }
            return acc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static IEnumerable<long> PrimeFactors3(long n)
        {
            return FindFactors(n, TWO, new List<long>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<long> Divisors()
        {
            yield return 2;
            var result = 3L;
            while (true)
            {
                yield return result;
                result += TWO;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        private static Tuple<long, long> Generator(long current)
        {
            return Tuple.Create(current, current == TWO ? THREE : current + TWO);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<long> Divisors2()
        {
            return TWO.Unfold(Generator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="divisors"></param>
        /// <returns></returns>
        private static IEnumerable<long> Factor(long n, IEnumerator<long> divisors)
        {
            var p = divisors.Current;
            if (p * p > n)
            {
                yield return n;
            }
            else if (n % p == 0)
            {
                yield return p;
                foreach (var item in Factor(n / p, divisors))
                {
                    yield return item;
                }
            }
            else
            {
                divisors.MoveNext();
                foreach (var item in Factor(n, divisors))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="divisors"></param>
        /// <returns></returns>
        private static IEnumerable<long> Factor2(long n, IEnumerator<long> divisors)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static IEnumerable<long> PrimeFactors(long n)
        {
            var d = Divisors2().GetEnumerator();
            d.MoveNext();
            return Factor(n, d);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long LargestPrimeFactor(long n)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long LargestPrimeFactor2(long n)
        {
            var divisor = 2L;
            var result = 1L;
            while (n > 1)
            {
                while(n % divisor == 0)
                {
                    result = divisor;
                    n /= divisor;
                }
                divisor += divisor == TWO ? 1 : TWO;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long LargestPrimeFactor3(long n)
        {
            var divisor = 2L;
            var result = 1L;
            while (divisor * divisor <= n)
            {
                while (n % divisor == 0)
                {
                    result = divisor;
                    n /= divisor;
                }
                divisor += divisor == TWO ? 1 : TWO;
            }
            return n == 1 ? result : n;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long LargestPrimeFactor4(long n)
        {
            var divisor = 2L;
            var result = 1L;
            var addend = 2L;
            while (divisor * divisor <= n)
            {
                while (n % divisor == 0)
                {
                    result = divisor;
                    n /= divisor;
                }
                switch(divisor) 
                {
                    case TWO:
                        divisor = THREE;
                        break;
                    case THREE:
                        divisor = FIVE;
                        break;
                    default:
                        divisor += addend;
                        addend = addend == TWO ? FOUR : TWO;
                        break;
                }
            }
            return n == 1 ? result : n;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long LargestPrimeFactor5(long n)
        {
            var divisor = 2L;
            var result = 1L;
            var gaps = new[] { 4L, 2L, 4L, 2L, 4L, 6L, 2L, 6L };
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
                    case TWO:
                        divisor = THREE;
                        break;
                    case THREE:
                        divisor = FIVE;
                        break;
                    case FIVE:
                        divisor = SEVEN;
                        break;
                    default:
                        divisor += gaps[i];
                        i = (i + 1) % gaps.Length;
                        break;
                }
            }
            return n == 1 ? result : n;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var experiments = new Function[]
            {
                LargestPrimeFactor,
                LargestPrimeFactor2,
                LargestPrimeFactor3,
                LargestPrimeFactor4,
                LargestPrimeFactor5,
            };
            const long N = 600851475143;
            Console.WriteLine(string.Join(", ", PrimeFactors(N)));
            return;

            foreach (var result in experiments
                .Select(func => new { result = func(N), time = Benchmark(() => func(N), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
            {
                Console.WriteLine("{0,-30} {1} {2:0.0000}", result.name, result.result, result.time);
            }
        }
    }
}
