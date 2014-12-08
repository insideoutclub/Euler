#include <windows.h>
#include <boost/range/algorithm/sort.hpp>
#include <boost/range/algorithm/equal.hpp>
#include <boost/range/adaptor/reversed.hpp>
#include <iostream>
#include <iomanip>
#include <vector>
#include <boost/format.hpp>

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

static bool isPalindromeString(int x)
{
    auto s = (boost::format("%1%") % x).str();
    return boost::equal(s, s | boost::adaptors::reversed);
}

static int reverse(int x)
{
    auto result = 0;
    while (x > 0)
    {
        result = 10 * result + x % 10;
        x /= 10;
    }
    return result;
}

static bool isPalindrome(int x)
{
    return x == reverse(x);
}

static int iterativeString()
{
    auto maximum = 0;
    for(auto i = 100; i != 1000; ++i)
        for (auto j = i; j != 1000; ++j)
        {
            auto product = i * j;
            if (isPalindromeString(product) && product > maximum)
                maximum = product;
        }
    return maximum;
}

static int iterativeMath()
{
    auto maximum = 0;
    for (auto i = 100; i != 1000; ++i)
        for (auto j = i; j != 1000; ++j)
        {
            auto product = i * j;
            if (isPalindrome(product) && product > maximum)
                maximum = product;
        }
    return maximum;
}

static int iterativeMath2()
{
    auto maximum = 0;
    for (auto i = 999; i != 99; --i)
        for (auto j = 999; j != i; --j)
        {
            auto product = i * j;
            if (product <= maximum)
                break;
            if (isPalindrome(product))
                maximum = product;
        }
    return maximum;
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
    Benchmark(Functor(functor), 10)))

int main()
{
    auto results = std::vector<Result>();

    RUN_EXPERIMENT(iterativeString);
    RUN_EXPERIMENT(iterativeMath);
    RUN_EXPERIMENT(iterativeMath2);

    auto const byTime = [](Result const& x, Result const& y) { return x.time < y.time; };

    boost::sort(results, byTime);
    for(auto first = std::begin(results), last = std::end(results); first != last; ++first)
        std::cout << std::setw(30) << std::left << first->name << " " << first->answer << " " <<
            std::fixed << std::setprecision(4) << first->time << "\n";
}
