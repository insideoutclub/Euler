using System;
using System.Diagnostics;
using System.Linq;
using Function = System.Func<int>;

namespace _04CSharp
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

        private static bool isPalindromeString(int x)
        {
            var s = x.ToString();
            return s.SequenceEqual(s.Reverse());
        }

        private static int reverse(int x)
        {
            var result = 0;
            while (x > 0)
            {
                result = 10 * result + x % 10;
                x /= 10;
            }
            return result;
        }

        private static bool isPalindrome(int x)
        {
            return x == reverse(x);
        }

        private static int iterativeString()
        {
            var maximum = int.MinValue;
            for(var i = 100; i != 1000; ++i)
                for (var j = i; j != 1000; ++j)
                {
                    var product = i * j;
                    if (isPalindromeString(product) && product > maximum)
                        maximum = product;
                }
            return maximum;
        }

        private static int iterativeMath()
        {
            var maximum = int.MinValue;
            for (var i = 100; i != 1000; ++i)
                for (var j = i; j != 1000; ++j)
                {
                    var product = i * j;
                    if (isPalindrome(product) && product > maximum)
                        maximum = product;
                }
            return maximum;
        }

        private static int iterativeMath2()
        {
            var maximum = int.MinValue;
            for (var i = 999; i != 99; --i)
                for (var j = 999; j != i; --j)
                {
                    var product = i * j;
                    if (product <= maximum)
                        break;
                    if (isPalindrome(product))
                        maximum = product;
                }
            return maximum;
        }

        private static int functional()
        {
            return (from i in Enumerable.Range(100, 900)
                    from j in Enumerable.Range(i, 1000 - i)
                    let product = i * j
                    where isPalindromeString(product)
                    select product).Max();
        }

        static void Main(string[] args)
        {
            var experiments = new Function[]
            {
                iterativeString,
                iterativeMath,
                iterativeMath2,
                functional,
            };
            foreach (var result in experiments
                .Select(func => new { result = func(), time = Benchmark(() => func(), 10), name = func.Method.Name })
                .OrderBy(tuple => tuple.time))
            {
                Console.WriteLine("{0,-30} {1} {2:0.0000}", result.name, result.result, result.time);
            }
        }
    }
}
