#include <windows.h>
#include <boost/range/irange.hpp>
#include <boost/range/numeric.hpp>
#include <boost/range/adaptor/filtered.hpp>
#include <boost/range/algorithm/sort.hpp>
#include <boost/range/algorithm/for_each.hpp>
#include <iostream>
#include <iomanip>
#include <vector>

// Returns true if dividend is a multiple of divisor
struct IsMultipleOf {
    explicit IsMultipleOf(int const divisor = int()) : divisor(divisor) {}
    bool operator()(int const dividend) const { return dividend % divisor == 0; }
    int const divisor;
};

// Returns true if dividend is a multiple of either divisor0 or divisor1
struct IsMultipleOfEither {
    explicit IsMultipleOfEither(int const divisor0 = int(), int const divisor1 = int())
        : isMultipleOfDivisor0(divisor0), isMultipleOfDivisor1(divisor1) {}
    bool operator()(int const dividend) const {
        return isMultipleOfDivisor0(dividend) || isMultipleOfDivisor1(dividend);
    }
    IsMultipleOf isMultipleOfDivisor0;
    IsMultipleOf isMultipleOfDivisor1;
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

// Generate [1, limit)
// Filter out multiples of x and y
// Returns sum
static int FilterImperative(int limit, int x, int y)
{
  auto sum = 0;
  auto const isMultipleOfXOrY = IsMultipleOfEither(x, y);
  for (auto i = 1; i < limit; ++i)
    if (isMultipleOfXOrY(i))
      sum += i;
  return sum;
}

// Sum multiples of x
// Sum multiples of y
// Subtract multiples of x * y to eliminate duplicates
static int GenerateMultiplesImperative(int limit, int x, int y)
{
  auto sum = 0;
  for (auto i = x; i < limit; i += x) sum += i;
  for (auto i = y; i < limit; i += y) sum += i;
  for (auto i = x * y; i < limit; i += x * y) sum -= i;
  return sum;
}

// Compute the sum of [x, limit) step x using triangular numbers
static int SumOfMultiplesOf(int x, int limit)
{
  auto p = (limit - 1) / x;
  return x * p * (p + 1) / 2;
}

// Sum multiples of x
// Sum multiples of y
// Subtract multiples of x * y to eliminate duplicates
static int SumOfMultiples(int limit, int x, int y)
{
  return SumOfMultiplesOf(x, limit) + SumOfMultiplesOf(y, limit) - SumOfMultiplesOf(x * y, limit);
}

// Generate [1, limit)
// Filter out multiples of x and y
// Returns sum
static int FilterFunctional(int limit, int x, int y)
{
  return boost::accumulate(boost::irange(1, limit) | boost::adaptors::filtered(IsMultipleOfEither(x, y)), 0);
}

// Generate multiples of x up to but not including limit
static boost::strided_integer_range<int> MultiplesOf(int x, int limit)
{
  return boost::irange(x, limit, x);
}

// Sum multiples of x
// Sum multiples of y
// Subtract multiples of x * y to eliminate duplicates
static int GenerateMultiplesFunctional(int limit, int x, int y)
{
  return boost::accumulate(MultiplesOf(x, limit), 0) +
         boost::accumulate(MultiplesOf(y, limit), 0) -
         boost::accumulate(MultiplesOf(x * y, limit), 0);
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
    typedef int (*Function)(int limit, int x, int y);

    Functor() {}
    Functor(Function function, int limit, int x, int y) : function(function), limit(limit), x(x), y(y) {}

    int operator()() const { return function(limit, x, y); }

private:
    Function function;
    int limit;
    int x;
    int y;
};

#define RUN_EXPERIMENT(functor) results.emplace_back(Result( \
    #functor, \
    functor(limit, x, y), \
    Benchmark(Functor(functor, limit, x, y), 1000)))

int main()
{
  auto const limit = 1000, x = 3, y = 5;

  auto results = std::vector<Result>();

  RUN_EXPERIMENT(FilterImperative);
  RUN_EXPERIMENT(GenerateMultiplesImperative);
  RUN_EXPERIMENT(SumOfMultiples);
  RUN_EXPERIMENT(FilterFunctional);
  RUN_EXPERIMENT(GenerateMultiplesFunctional);

  auto const byTime = [](Result const& x, Result const& y) { return x.time < y.time; };

  boost::for_each(boost::sort(results, byTime), [](Result const& result) {
    std::cout << std::setw(30) << std::left << result.name << " " << result.answer << " " <<
        std::fixed << std::setprecision(4) << result.time << "\n"; });
}
