#include <windows.h>
#include <boost/range/irange.hpp>
#include <boost/range/numeric.hpp>
#include <boost/range/adaptor/filtered.hpp>
#include <boost/range/algorithm/sort.hpp>
#include <iostream>
#include <iomanip>

// Returns true if x is a multiple of y
struct IsMultipleOf {
  bool operator()(int const x, int const y) const { return x % y == 0; }
};

struct Stopwatch
{
  Stopwatch() : start{now()} {}

  double elapsed() const {
    auto finish = now();
    static const auto freq = frequency();
    return double(finish - start) / freq * 1000;
  }

private:
  LONGLONG now() const {
    LARGE_INTEGER result;
    QueryPerformanceCounter(&result);
    return result.QuadPart;
  }
  LONGLONG frequency() const {
    LARGE_INTEGER frequency;
    QueryPerformanceFrequency(&frequency);
    return frequency.QuadPart;
  }
  LONGLONG start;
};

// Run action the number of times specified by iterations
// Returns the elapsed time in milliseconds
template<typename Action>
static double Benchmark(Action action, int iterations)
{
  action();

  auto stopwatch = Stopwatch{};
  for (auto i = 0; i != iterations; ++i)
      action();
  return stopwatch.elapsed();
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
  auto sum = 0;
  for (auto i = x; i < limit; i += x) sum += i;
  return sum;
}

// Sum multiples of x
// Sum multiples of y
// Subtract multiples of x * y to eliminate duplicates
static int GenerateMultiplesImperative2(int limit, int x, int y)
{
  return SumMultiples(x, limit) + SumMultiples(y, limit) - SumMultiples(x * y, limit);
}

// Generate [1, limit)
// Filter out multiples of x and y
// Returns sum
static int FilterInlineFunctional(int limit, int x, int y)
{
  auto const multipleOfXOrY = [x, y](auto const i) { return i % x == 0 || i % y == 0; };
  return boost::accumulate(boost::irange(1, limit)
                         | boost::adaptors::filtered(multipleOfXOrY), 0);
}

// Same as FilterInlineFunctional, except multiples filter is a function call
static int FilterFunctionCallFunctional(int limit, int x, int y)
{
  auto const multipleOfXOrY = [x, y](auto const i) {
    return IsMultipleOf()(i, x) || IsMultipleOf()(i, y); };
  return boost::accumulate(boost::irange(1, limit)
                           | boost::adaptors::filtered(multipleOfXOrY), 0);
}

// Generate multiples of x up to but not including limit
static auto MultiplesOf(int x, int limit)
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
  double time;

  Result() {}
  Result(char const * const name, int answer, double time) :
    name(name), answer(answer), time(time) {}
};

int main()
{
  const struct {
    char const* name;
    int (*functor)(int limit, int x, int y);
  } experiments[] = {
    {"FilterInlineImperative", FilterInlineImperative},
    {"FilterFunctionCallImperative", FilterFunctionCallImperative},
    {"GenerateMultiplesImperative", GenerateMultiplesImperative},
    {"GenerateMultiplesImperative2", GenerateMultiplesImperative2},
    {"FilterInlineFunctional", FilterInlineFunctional},
    {"FilterFunctionCallFunctional", FilterFunctionCallFunctional},
    {"GenerateMultiplesFunctional", GenerateMultiplesFunctional},
  };

  auto const limit = 1000, x = 3, y = 5;

  auto results = std::vector<Result>{};
  results.reserve(std::end(experiments) - std::begin(experiments));

  for (auto const& experiment: experiments)
    results.emplace_back(
      experiment.name,
      experiment.functor(limit, x, y),
      Benchmark([&experiment]() { experiment.functor(limit, x, y); }, 1000));

  auto const sortByTime = [](auto const& x, auto const& y) { return x.time < y.time; };
  boost::sort(results, sortByTime);

  for (auto const& result: results)
    std::cout << std::setw(30) << std::left << result.name << " " << result.answer << " " <<
      result.time << "\n";
}
