#include <windows.h>
#include <boost/range/algorithm/sort.hpp>
#include <iostream>
#include <iomanip>
#include <vector>
#include <tuple>
#include <cstdint>

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

static int64_t largestPrimeFactor(int64_t n) {
    auto divisor = 2;
    auto result = 1;
    while(n > 1) {
        while(n % divisor == 0) {
            result = divisor;
            n /= divisor;
        }
        ++divisor;
    }
    return result;
}

static int64_t largestPrimeFactor2(int64_t n) {
    auto divisor = 2;
    auto result = 1;
    while (n > 1) {
        while(n % divisor == 0) {
            result = divisor;
            n /= divisor;
        }
        divisor += divisor == 2 ? 1 : 2;
    }
    return result;
}

static int64_t largestPrimeFactor3(int64_t n) {
    auto divisor = 2;
    auto result = 1;
    while (n > 1 && divisor * divisor <= n) {
        while (n % divisor == 0) {
            result = divisor;
            n /= divisor;
        }
        divisor += divisor == 2 ? 1 : 2;
    }
    return n == 1 ? result : n;
}

static int64_t largestPrimeFactor4(int64_t n) {
    auto divisor = 2;
    auto result = 1;
    auto addend = 2;
    while (n > 1 && divisor * divisor <= n) {
        while (n % divisor == 0) {
            result = divisor;
            n /= divisor;
        }
        switch(divisor) {
            case 2: divisor = 3; break;
            case 3: divisor = 5; break;
            default: divisor += addend; addend = addend == 2 ? 4 : 2; break;
        }
    }
    return n == 1 ? result : n;
}

struct Result {
    Result() {}
    Result(char const * const name, int64_t answer, double time) : name(name), answer(answer), time(time) {}

    char const* name;
    int64_t answer;
    double time;
};

// DO NOT TRY THIS AT HOME!!!!
// This is a pessimization to prevent inlining.
// Otherwise, the compiler detects that the functions have no side effects and the benchmarks take zero time.
// Calling through a function pointer forces the compiler to execute the code.
struct Functor {
    typedef int64_t (*Function)(int64_t n);

    Functor() {}
    Functor(Function const function, int64_t const n) : function(function),  n(n) {}

    int64_t operator()() const { return function(n); }

private:
    Function function;
    int64_t n;
};

#define RUN_EXPERIMENT(functor) results.emplace_back(Result( \
    #functor, \
    functor(n), \
    Benchmark(Functor(functor, n), 1000)))

int main()
{
    auto const n = 600851475143;

    auto results = std::vector<Result>();

    RUN_EXPERIMENT(largestPrimeFactor);
    RUN_EXPERIMENT(largestPrimeFactor2);
    RUN_EXPERIMENT(largestPrimeFactor3);
    RUN_EXPERIMENT(largestPrimeFactor4);

    auto const byTime = [](Result const& x, Result const& y) { return x.time < y.time; };

    boost::sort(results, byTime);
    for(auto first = std::begin(results), last = std::end(results); first != last; ++first)
        std::cout << std::setw(30) << std::left << first->name << " " << first->answer << " " <<
            std::fixed << std::setprecision(4) << first->time << "\n";
}
