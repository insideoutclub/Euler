using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _03CSharp
{
    class Program
    {
        static IEnumerable<int> divisors()
        {
            yield return 2;
            yield return 3;
            var result = 5;
            yield return result;
            for (var addend = 2; true; addend = addend == 2 ? 4 : 2)
                yield return result += addend;
        }

        static IEnumerable<int> primeFactors(int n)
        {
            var d = divisors().GetEnumerator();
            d.MoveNext();

            A2:
            if (n == 1) yield break;
            int r;

            A3:
            var q = Math.DivRem(n, d.Current, out r);
            if(r != 0)
                goto A6;
            yield return d.Current;
            n = q;
            goto A2;

            A6:
            if(q > d.Current) {
                d.MoveNext();
                goto A3;
            }

            yield return n;
        }

        static void Main(string[] args)
        {
            Console.WriteLine(string.Join(", ", primeFactors(13195)));
        }
    }
}
