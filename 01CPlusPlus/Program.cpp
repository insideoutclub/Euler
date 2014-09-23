#include <windows.h>
#include <boost/range/irange.hpp>
#include <boost/range/numeric.hpp>
#include <boost/range/adaptor/filtered.hpp>
#include <boost/range/algorithm/sort.hpp>
#include <boost/range/algorithm/for_each.hpp>
#include <iostream>
#include <iomanip>
#include <vector>

// Returns true if x is a multiple of y
struct IsMultipleOf {
  bool operator()(int const x, int const y) const { return x % y == 0; }
};

// Run action the number of times specified by iterations
// Returns the elapsed time in milliseconds
template<typename Action>
static __int64 Benchmark(Action action, int iterations)
{
  auto start = LARGE_INTEGER();
  QueryPerformanceCounter(&start);

  for (auto i = 0; i != iterations; ++i)
      action();

  auto finish = LARGE_INTEGER();
  QueryPerformanceCounter(&finish);
  return finish.QuadPart - start.QuadPart;
}

// Generate [1, limit)
// Filter out multiples of x and y
// Returns sum
static int FilterInlineImperative(int limit, int x, int y)
{
  auto sum = 0;
  for (auto i = 1; i < limit; ++i)
    if (i % x == 0 || i % y == 0)
      sum += i;
  return sum;
}

// Same as FilterInlineImperative, except multiples filter is a function call
static int FilterFunctionCallImperative(int limit, int x, int y)
{
  auto sum = 0;
  for (auto i = 1; i < limit; ++i)
    if (IsMultipleOf()(i, x) || IsMultipleOf()(i, y))
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

static int SumMultiples(int x, int limit)
{
  auto p = (limit - 1) / x;
  return x * p * (p + 1) / 2;
}

// Sum multiples of x
// Sum multiples of y
// Subtract multiples of x * y to eliminate duplicates
static int SumMultiples(int limit, int x, int y)
{
  return SumMultiples(x, limit) + SumMultiples(y, limit) - SumMultiples(x * y, limit);
}

// Generate [1, limit)
// Filter out multiples of x and y
// Returns sum
static int FilterInlineFunctional(int limit, int x, int y)
{
  auto const multipleOfXOrY = [x, y](int const i) { return i % x == 0 || i % y == 0; };
  return boost::accumulate(boost::irange(1, limit)
                         | boost::adaptors::filtered(multipleOfXOrY), 0);
}

// Same as FilterInlineFunctional, except multiples filter is a function call
static int FilterFunctionCallFunctional(int limit, int x, int y)
{
  auto const multipleOfXOrY = [x, y](int const i) {
    return IsMultipleOf()(i, x) || IsMultipleOf()(i, y); };
  return boost::accumulate(boost::irange(1, limit)
                           | boost::adaptors::filtered(multipleOfXOrY), 0);
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
  char const* name;
  int answer;
  __int64 time;

  Result() {}
  Result(char const * const name, int answer, __int64 time) :
    name(name), answer(answer), time(time) {}
};

#define RUN_EXPERIMENT(functor) results.emplace_back(Result(#functor, functor(limit, x, y), Benchmark([limit, x, y]() { functor(limit, x, y); }, 1000)))

int main()
{
  auto const limit = 1000, x = 3, y = 5;

  auto results = std::vector<Result>();
  RUN_EXPERIMENT(FilterInlineImperative);
  RUN_EXPERIMENT(FilterFunctionCallImperative);
  RUN_EXPERIMENT(GenerateMultiplesImperative);
  RUN_EXPERIMENT(SumMultiples);
  RUN_EXPERIMENT(FilterInlineFunctional);
  RUN_EXPERIMENT(FilterFunctionCallFunctional);
  RUN_EXPERIMENT(GenerateMultiplesFunctional);

  auto const sortByTime = [](Result const& x, Result const& y) { return x.time < y.time; };
  boost::for_each(boost::sort(results, sortByTime), [](Result const& result) {
    std::cout << std::setw(30) << std::left << result.name << " " << result.answer << " " <<
        result.time << "\n"; });
}
