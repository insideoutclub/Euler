#include <windows.h>
#include <boost/iterator/iterator_facade.hpp>
#include <boost/range/iterator_range.hpp>
#include <boost/range/numeric.hpp>
#include <boost/range/adaptor/filtered.hpp>
#include <boost/range/algorithm/for_each.hpp>
#include <boost/range/algorithm/sort.hpp>
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

struct fibonacci_iterator
    : public boost::iterator_facade<fibonacci_iterator, int, boost::forward_traversal_tag, int> {

    explicit fibonacci_iterator(int const a = int(), int const b = int(), int limit = int()) : a(a), b(b), limit(limit) {}

private:
    friend boost::iterator_core_access;

    int dereference() const { return a; }
    bool equal(fibonacci_iterator const&) const { return a > limit; }
    void increment() { std::tie(a, b) = nextFibonacci(a, b); }
    int a;
    int b;
    int limit;
};

boost::iterator_range<fibonacci_iterator> fibonacci_range(int const a, int const b, int const limit) {
    return boost::make_iterator_range(fibonacci_iterator(a, b, limit), fibonacci_iterator());
}

struct IsEven {
    bool operator()(int const x) const { return !(x & 1); }
} isEven;

static int filterImperative() {
    auto sum = 0;
    for(auto a = 1, b = 2; a <= 4000000; std::tie(a, b) = nextFibonacci(a, b))
        if (isEven(a))
            sum += a;
    return sum;
}

std::tuple<int, int> nextEvenFibonacci(int const a, int const b) { return std::make_tuple(b, a + 4 * b); }

static int noFilterImperative() {
    auto sum = 0;
    for (auto a = 2, b = 8; a <= 4000000; std::tie(a, b) = nextEvenFibonacci(a, b))
        sum += a;
    return sum;
}

static int filterFunctional() {
    return boost::accumulate(fibonacci_range(1, 2, 4000000) | boost::adaptors::filtered(isEven), 0);
}

struct even_fibonacci_iterator
    : public boost::iterator_facade<even_fibonacci_iterator, int, boost::forward_traversal_tag, int> {

    explicit even_fibonacci_iterator(int const a = int(), int const b = int(), int const limit = int()) : a(a), b(b), limit(limit) {}

private:
    friend boost::iterator_core_access;

    int dereference() const { return a; }
    bool equal(even_fibonacci_iterator const &) const { return a > limit; }
    void increment() { std::tie(a, b) = nextEvenFibonacci(a, b); }
    int a;
    int b;
    int limit;
};

boost::iterator_range<even_fibonacci_iterator> even_fibonacci_range(int const a, int const b, int const limit) {
    return boost::make_iterator_range(even_fibonacci_iterator(a, b, limit), even_fibonacci_iterator());
}

static int noFilterFunctional() {
    return boost::accumulate(even_fibonacci_range(2, 8, 4000000), 0);
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
    Functor(Function const function) : function(function) {}

    int operator()() const { return function(); }

private:
    Function function;
};

#define RUN_EXPERIMENT(functor) results.emplace_back(Result( \
    #functor, \
    functor(), \
    Benchmark(Functor(functor), 100000)))

int main()
{
    auto results = std::vector<Result>();

    RUN_EXPERIMENT(filterFunctional);
    RUN_EXPERIMENT(filterImperative);
    RUN_EXPERIMENT(noFilterImperative);
    RUN_EXPERIMENT(noFilterFunctional);

    auto const byTime = [](Result const& x, Result const& y) { return x.time < y.time; };

    boost::for_each(boost::sort(results, byTime), [](Result const& result) {
        std::cout << std::setw(30) << std::left << result.name << " " << result.answer << " " <<
            std::fixed << std::setprecision(4) << result.time / 100 << "\n"; });
}
