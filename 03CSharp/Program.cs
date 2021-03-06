﻿// Copyright (C) 2014 Teradyne, Inc. All rights reserved.
//
// This document contains proprietary and confidential information of Teradyne,
// Inc. and is tendered subject to the condition that the informaipation (a) be
// retained in confidence and (b) not be used or incorporated in any product
// except with the express written consent of Teradyne, Inc.
//
// Revision History:

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        /// <param name="current"></param>
        /// <returns></returns>
        private static Tuple<long, long> Skip2sGenerator(long current)
        {
            return Tuple.Create(current, current == TWO ? THREE : current + TWO);
        }

        /// <summary>
        /// 
        /// </summary>
        private struct Skip2sAnd3s
        {
            private readonly long _divisor;
            private readonly long _addend;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="divisor"></param>
            /// <param name="addend"></param>
            internal Skip2sAnd3s(long divisor, long addend)
            {
                _divisor = divisor;
                _addend = addend;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="state"></param>
            /// <returns></returns>
            internal static Tuple<long, Skip2sAnd3s> Skip2sAnd3sGenerator(Skip2sAnd3s state)
            {
                return Tuple.Create(state._divisor,
                    state._divisor == TWO ? new Skip2sAnd3s(THREE, 0) :
                    state._divisor == THREE ? new Skip2sAnd3s(FIVE, TWO) :
                    new Skip2sAnd3s(state._divisor + state._addend, state._addend == TWO ? FOUR : TWO));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private struct Skip2s3sAnd5s
        {
            private readonly long _divisor;
            private readonly static long[] _gaps = new[] { 4L, 2L, 4L, 2L, 4L, 6L, 2L, 6L };
            private readonly int _i;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="divisor"></param>
            /// <param name="i"></param>
            internal Skip2s3sAnd5s(long divisor, int i)
            {
                _divisor = divisor;
                _i = i;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="state"></param>
            /// <returns></returns>
            internal static Tuple<long, Skip2s3sAnd5s> Skip2s3sAnd5sGenerator(Skip2s3sAnd5s state)
            {
                return Tuple.Create(state._divisor,
                    state._divisor == TWO ? new Skip2s3sAnd5s(THREE, 0) :
                    state._divisor == THREE ? new Skip2s3sAnd5s(FIVE, 0) :
                    state._divisor == FIVE ? new Skip2s3sAnd5s(SEVEN, 0) :
                    new Skip2s3sAnd5s(state._divisor + _gaps[state._i], (state._i + 1) % _gaps.Length));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="State"></typeparam>
        /// <param name="n"></param>
        /// <param name="state"></param>
        /// <param name="nextDivisor"></param>
        /// <returns></returns>
        private static IEnumerable<long> Factor<State>(long n, State state, Func<State, Tuple<long, State>> nextDivisor)
        {
            var divisorAndState = nextDivisor(state);
            var divisor = divisorAndState.Item1;
            return divisor * divisor > n ? Enumerable.Repeat(n, 1) :
                n % divisor == 0 ? Enumerable.Repeat(divisor, 1).Concat(Factor(n / divisor, state, nextDivisor)) :
                Factor(n, divisorAndState.Item2, nextDivisor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long LargestPrimeFactorFunctional0(long n)
        {
            return Factor(n, TWO, (i => Tuple.Create(i, i + 1))).Last();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long LargestPrimeFactorFunctional1(long n)
        {
            return Factor(n, TWO, Skip2sGenerator).Last();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long LargestPrimeFactorFunctional2(long n)
        {
            return Factor(n, new Skip2sAnd3s(TWO, 0), Skip2sAnd3s.Skip2sAnd3sGenerator).Last();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long LargestPrimeFactorFunctional3(long n)
        {
            return Factor(n, new Skip2s3sAnd5s(TWO, 0), Skip2s3sAnd5s.Skip2s3sAnd5sGenerator).Last();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long LargestPrimeFactor(long n)
        {
            var divisor = 2L;
            var result = 1L;
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
                divisor = divisor == TWO ? THREE : divisor + TWO;
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
                divisor = divisor == TWO ? THREE : divisor + TWO;
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
                LargestPrimeFactorFunctional0,
                LargestPrimeFactorFunctional1,
                LargestPrimeFactorFunctional2,
                LargestPrimeFactorFunctional3,
            };
            const long N = 600851475143;

            foreach (var result in experiments
                .Select(func => new { result = func(N), time = Benchmark(() => func(N), 1000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
            {
                Console.WriteLine("{0,-30} {1} {2:0.0000}", result.name, result.result, result.time);
            }
        }
    }
}
