#include <windows.h>
#include <boost/range/algorithm/sort.hpp>
#include <boost/range/numeric.hpp>
#include <boost/range/irange.hpp>
#include <iostream>
#include <iomanip>
#include <vector>
#include <tuple>

int brute_force()
{
    for (auto i = 1; true; ++i)
    {
        auto j = 2;
        for (; j != 21; ++j)
            if (i % j != 0)
                break;
        if (j == 21)
            return i;
    }
}

int gcd(int a, int b)
{
    while (b != 0)
        std::tie(a, b) = std::make_tuple(b, a % b);
    return a;
}

int lcm(int a, int b)
{
    return abs(a) / gcd(a, b) * abs(b);
}

int imperative()
{
    return boost::accumulate(boost::irange(2, 20), 1, [](int acc, int x) { return lcm(acc, x); });
}

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
    typedef int(*Function)();

    Functor() {}
    Functor(Function const function) : function(function) {}

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

    //RUN_EXPERIMENT(brute_force);
    RUN_EXPERIMENT(imperative);

    auto const byTime = [](Result const& x, Result const& y) { return x.time < y.time; };

    boost::sort(results, byTime);
    for (auto first = std::begin(results), last = std::end(results); first != last; ++first)
        std::cout << std::setw(30) << std::left << first->name << " " << first->answer << " " <<
        std::fixed << std::setprecision(4) << first->time << "\n";
}
