#include <windows.h>
#include <boost/range/irange.hpp>
#include <boost/range/numeric.hpp>
#include <boost/range/adaptor/filtered.hpp>
#include <boost/range/algorithm/sort.hpp>
#include <boost/range/algorithm/for_each.hpp>
#include <iostream>
#include <iomanip>
#include <vector>
#include <range/v3/numeric/accumulate.hpp>
#include <range/v3/view/filter.hpp>
#include <range/v3/view/iota.hpp>
#include <range/v3/algorithm/sort.hpp>
#include <range/v3/algorithm/for_each.hpp>

// Returns true if i is a multiple of 3 or 5
struct IsMultipleOf3Or5 {
    bool operator()(int const i) const { return i % 3 == 0 || i % 5 == 0; }
};

// Run action the number of times specified by iterations
// Returns the elapsed time in milliseconds
template<typename Action>
static double Benchmark(Action action, int iterations)
{
  auto start = LARGE_INTEGER();
  QueryPerformanceCounter(&start);

  for (auto i = 0; i != iterations; ++i)
      action();

  auto finish = LARGE_INTEGER();
  QueryPerformanceCounter(&finish);

  auto frequency = LARGE_INTEGER();
  QueryPerformanceFrequency(&frequency);

  return double(finish.QuadPart - start.QuadPart) * 1000 / frequency.QuadPart;
}

// Generate [1, 1000)
// Filter out multiples of 3 and 5
// Returns sum
static int FilterImperative()
{
  auto sum = 0;
  for (auto i = 1; i < 1000; ++i)
    if (IsMultipleOf3Or5()(i))
      sum += i;
  return sum;
}

// Sum multiples of 3
// Sum multiples of 5
// Subtract multiples of 3 * 5 to eliminate duplicates
static int GenerateMultiplesImperative()
{
  auto sum = 0;
  for (auto i = 3; i < 1000; i += 3) sum += i;
  for (auto i = 5; i < 1000; i += 5) sum += i;
  for (auto i = 3 * 5; i < 1000; i += 3 * 5) sum -= i;
  return sum;
}

// Compute the sum of [x, 1000) step x using triangular numbers
static int SumOfMultiplesOf(int x)
{
  auto p = (1000 - 1) / x;
  return x * p * (p + 1) / 2;
}

// Sum multiples of 3
// Sum multiples of 5
// Subtract multiples of 3 * 5 to eliminate duplicates
static int SumOfMultiples()
{
  return SumOfMultiplesOf(3) + SumOfMultiplesOf(5) - SumOfMultiplesOf(3 * 5);
}

// Generate [1, 1000)
// Filter out multiples of 3 and 5
// Returns sum
static int FilterFunctional()
{
  return ranges::accumulate(ranges::view::ints(1, 999) | ranges::view::filter(IsMultipleOf3Or5()), 0);
}

// Generate multiples of x up to but not including 1000
static boost::strided_integer_range<int> MultiplesOf(int x)
{
  return boost::irange(x, 1000, x);
}

// Sum multiples of 3
// Sum multiples of 5
// Subtract multiples of 3 * 5 to eliminate duplicates
static int GenerateMultiplesFunctional()
{
  return boost::accumulate(MultiplesOf(3), 0) +
         boost::accumulate(MultiplesOf(5), 0) -
         boost::accumulate(MultiplesOf(3 * 5), 0);
}

struct Result {
    Result() {}
    Result(char const * const name, int answer, double time) : name(name), answer(answer), time(time) {}

    char const* name;
    int answer;
    double time;
};

// DO NOT TRY THIS AT HOME!!!!
// This is a pessimization to prevent inlining.
// Otherwise, the compiler detects that the functions have no side effects and the benchmarks take zero time.
// Calling through a function pointer forces the compiler to execute the code.
struct Functor {
    typedef int (*Function)();

    Functor() {}
    Functor(Function function) : function(function) {}

    int operator()() const { return function(); }

private:
    Function function;
};

#define RUN_EXPERIMENT(functor) results.emplace_back(Result( \
    #functor, \
    functor(), \
    Benchmark(Functor(functor), 1000)))

int main()
{
  auto results = std::vector<Result>();

  RUN_EXPERIMENT(FilterImperative);
  RUN_EXPERIMENT(GenerateMultiplesImperative);
  RUN_EXPERIMENT(SumOfMultiples);
  RUN_EXPERIMENT(FilterFunctional);
  RUN_EXPERIMENT(GenerateMultiplesFunctional);

  auto const byTime = [](Result const& x, Result const& y) { return x.time < y.time; };

  ranges::sort(results, byTime);
  ranges::for_each(results, [](Result const& result) {
    std::cout << std::setw(30) << std::left << result.name << " " << result.answer << " " <<
        std::fixed << std::setprecision(4) << result.time << "\n"; });
}
