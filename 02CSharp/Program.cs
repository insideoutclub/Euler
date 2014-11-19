using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Function = System.Func<int>;

namespace _02CSharp
{
    static class Program
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

        static IEnumerable<TResult> Unfold<T, TResult>(this T next, Func<T, Tuple<TResult, T>> generator)
        {
            var result = Tuple.Create(default(TResult), next);
            while((result = generator(result.Item2)) != null)
                yield return result.Item1;
        }

        static Tuple<int, Tuple<int, int>> nextFibonacci(Tuple<int, int> state)
        {
            var nextValueInSequence = state.Item1;
            var nextState = Tuple.Create(state.Item2, state.Item1 + state.Item2);
            return Tuple.Create(nextValueInSequence, nextState);
        }

        static IEnumerable<int> fibonacci()
        {
            return Tuple.Create(1, 2).Unfold(nextFibonacci);
        }

        static int filterImperative()
        {
            int a = 1, b = 2, sum = 0;
            while (a <= 4000000)
            {
                if ((a & 1) == 0) sum += a;
                var aPlusB = a + b;
                a = b;
                b = aPlusB;
            }
            return sum;
        }

        static int noFilterImperative()
        {
            int a = 2, b = 8, sum = 0;
            while (a <= 4000000)
            {
                sum += a;
                var aPlus4b = a + 4 * b;
                a = b;
                b = aPlus4b;
            }
            return sum;
        }

        static bool isEven(int x) { return (x & 1) == 0; }

        static bool doesNotExceedFourMillion(int x) { return x <= 4000000; }

        static int filterFunctional()
        {
            return fibonacci().TakeWhile(doesNotExceedFourMillion).Where(isEven).Sum();
        }

        static Tuple<int, Tuple<int, int>> nextEvenFibonacci(Tuple<int, int> state)
        {
            return Tuple.Create(state.Item1, Tuple.Create(state.Item2, state.Item1 + 4 * state.Item2));
        }

        static IEnumerable<int> evenFibonacci()
        {
            return Tuple.Create(2, 8).Unfold(nextEvenFibonacci);
        }

        static int noFilterFunctional()
        {
            return evenFibonacci().TakeWhile(doesNotExceedFourMillion).Sum();
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
                .Select(func => new { result = func(), time = benchmark(() => func(), 100000), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
                Console.WriteLine("{0,-30} {1} {2:0.0000}", result.name, result.result, result.time / 100);
        }
    }
}
