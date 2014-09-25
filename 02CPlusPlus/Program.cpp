#include <windows.h>
#include <boost/range/irange.hpp>
#include <boost/range/numeric.hpp>
#include <boost/range/adaptor/filtered.hpp>
#include <boost/range/algorithm/sort.hpp>
#include <boost/range/algorithm/for_each.hpp>
#include <iostream>
#include <iomanip>
#include <vector>
#include <tuple>

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

struct FibonacciIterator : public boost::iterator_facade<FibonacciIterator, int, boost::forward_traversal_tag, int> {
    FibonacciIterator() {}
    FibonacciIterator(int a, int b) : a(a), b(b) {}

private:
    friend boost::iterator_core_access;

    int dereference() const { return a; }
    bool equal(FibonacciIterator const& that) const { return a >= that.a; }
    void increment() { std::tie(a, b) = nextFibonacci(a, b); }
    int a;
    int b;
};

boost::iterator_range<FibonacciIterator> fibonacciRange(int const a, int const b, int const limit) {
    return boost::make_iterator_range(FibonacciIterator(a, b), FibonacciIterator(limit, limit));
}

struct IsEven { bool operator()(int const x) { return x % 2 == 0; } };

static int filterImperative(int a, int b, int limit) {
    auto sum = 0;
    while (a < limit) {
        if (IsEven()(a)) sum += a;
        std::tie(a, b) = nextFibonacci(a, b);
    }
    return sum;
}

std::tuple<int, int> nextEvenFibonacci(int a, int b) { return std::make_tuple(b, a + 4 * b); }

static int noFilterImperative(int a, int b, int limit) {
    auto sum = 0;
    while (a < limit) {
        sum += a;
        std::tie(a, b) = nextEvenFibonacci(a, b);
    }
    return sum;
}

static int filterFunctional(int a, int b, int limit) {
    return boost::accumulate(fibonacciRange(a, b, limit) | boost::adaptors::filtered(IsEven()), 0);
}

struct EvenFibonacciIterator : public boost::iterator_facade<EvenFibonacciIterator, int, boost::forward_traversal_tag, int> {
    EvenFibonacciIterator() {}
    EvenFibonacciIterator(int a, int b) : a(a), b(b) {}

private:
    friend boost::iterator_core_access;

    int dereference() const { return a; }
    bool equal(EvenFibonacciIterator const& that) const { return a >= that.a; }
    void increment() { std::tie(a, b) = nextEvenFibonacci(a, b); }
    int a;
    int b;
};

boost::iterator_range<EvenFibonacciIterator> evenFibonacciRange(int const a, int const b, int const limit) {
    return boost::make_iterator_range(EvenFibonacciIterator(a, b), EvenFibonacciIterator(limit, limit));
}

static int noFilterFunctional(int a, int b, int limit) {
    return boost::accumulate(evenFibonacciRange(a, b, limit), 0);
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
    typedef int (*Function)(int x, int y, int limit);

    Functor() {}
    Functor(Function function, int x, int y, int limit) : function(function), x(x), y(y), limit(limit) {}

    int operator()() const { return function(x, y, limit); }

private:
    Function function;
    int x;
    int y;
    int limit;
};

#define RUN_EXPERIMENT(functor, x, y) results.emplace_back(Result( \
    #functor, \
    functor(x, y, limit), \
    Benchmark(Functor(functor, x, y, limit), 10000)))

int main()
{
    auto const limit = 4000001;

    auto results = std::vector<Result>();

    RUN_EXPERIMENT(filterFunctional, 1, 2);
    RUN_EXPERIMENT(filterImperative, 1, 2);
    RUN_EXPERIMENT(noFilterImperative, 2, 8);
    RUN_EXPERIMENT(noFilterFunctional, 2, 8);

    auto const byTime = [](Result const& x, Result const& y) { return x.time < y.time; };

    boost::for_each(boost::sort(results, byTime), [](Result const& result) {
        std::cout << std::setw(30) << std::left << result.name << " " << result.answer << " " <<
            std::fixed << std::setprecision(4) << result.time << "\n"; });
}
