#include <windows.h>
#include <iostream>
#include <iomanip>
#include <vector>
#include <tuple>
#include <range/v3/core.hpp>
#include <range/v3/view/take_while.hpp>
#include <range/v3/view/filter.hpp>
#include <range/v3/numeric/accumulate.hpp>
#include <range/v3/algorithm/sort.hpp>
#include <range/v3/algorithm/for_each.hpp>

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

std::tuple<int, int> nextFibonacci(int const a, int const b) { return std::make_tuple(b, a + b); }
std::tuple<int, int> previousFibonacci(int const a, int const b) { return std::make_tuple(b - a, a); }

struct fibonacci_range
    : public ranges::range_facade<fibonacci_range> {

    explicit fibonacci_range(int const a = int(), int const b = int()) : a(a), b(b) {}

private:
    friend ranges::range_access;

    int const & current() const { return a; }
    bool done() const { return false; }
    void next() { std::tie(a, b) = nextFibonacci(a, b); }
    void prev() { std::tie(a, b) = previousFibonacci(a, b); }
    int a;
    int b;
};

struct IsEven {
    bool operator()(int const x) const { return !(x & 1); }
} isEven;

static int filterImperative(int limit) {
    auto sum = 0;
    for(auto a = 1, b = 2; a < limit; std::tie(a, b) = nextFibonacci(a, b))
        if (isEven(a))
            sum += a;
    return sum;
}

std::tuple<int, int> nextEvenFibonacci(int const a, int const b) { return std::make_tuple(b, a + 4 * b); }
std::tuple<int, int> previousEvenFibonacci(int const a, int const b) { return std::make_tuple(b - 4 * a, a); }

static int noFilterImperative(int limit) {
    auto sum = 0;
    for (auto a = 2, b = 8; a < limit; std::tie(a, b) = nextEvenFibonacci(a, b))
        sum += a;
    return sum;
}

struct LessThanLimit {
    explicit LessThanLimit(int limit = int()) : limit(limit) {}
    bool operator()(int const x) const { return x < limit; }
    int limit;
};

static int filterFunctional(int const limit) {
    return ranges::accumulate(fibonacci_range(1, 2)
                           | ranges::view::filter(isEven)
                           | ranges::view::take_while(LessThanLimit(limit))
                           , 0);
}

struct even_fibonacci_range
    : public ranges::range_facade<even_fibonacci_range> {

    explicit even_fibonacci_range(int const a = int(), int const b = int()) : a(a), b(b) {}

private:
    friend ranges::range_access;

    int const & current() const { return a; }
    bool done() const { return false; }
    void next() { std::tie(a, b) = nextEvenFibonacci(a, b); }
    void prev() { std::tie(a, b) = previousEvenFibonacci(a, b); }
    int a;
    int b;
};

static int noFilterFunctional(int const limit) {
    return ranges::accumulate(even_fibonacci_range(2, 8) | ranges::view::take_while(LessThanLimit(limit)), 0);
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
    typedef int (*Function)(int const limit);

    Functor() {}
    Functor(Function const function, int const limit) : function(function),  limit(limit) {}

    int operator()() const { return function(limit); }

private:
    Function function;
    int limit;
};

#define RUN_EXPERIMENT(functor) results.emplace_back(Result( \
    #functor, \
    functor(limit), \
    Benchmark(Functor(functor, limit), 1000)))

int main()
{
    auto const limit = 4000001;

    auto results = std::vector<Result>();

    RUN_EXPERIMENT(filterFunctional);
    RUN_EXPERIMENT(filterImperative);
    RUN_EXPERIMENT(noFilterImperative);
    RUN_EXPERIMENT(noFilterFunctional);

    auto const byTime = [](Result const& x, Result const& y) { return x.time < y.time; };

    ranges::sort(results, byTime);
    ranges::for_each(results, [](Result const& result) {
        std::cout << std::setw(30) << std::left << result.name << " " << result.answer << " " <<
            std::fixed << std::setprecision(4) << result.time << "\n"; });
}
