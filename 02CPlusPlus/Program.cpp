#include <windows.h>
#include <boost/range/irange.hpp>
#include <boost/range/numeric.hpp>
#include <boost/range/adaptor/filtered.hpp>
#include <boost/range/algorithm/sort.hpp>
#include <boost/range/algorithm/for_each.hpp>
#include <boost/range/adaptor/argument_fwd.hpp>
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
std::tuple<int, int> previousFibonacci(int const a, int const b) { return std::make_tuple(b - a, a); }

struct fibonacci_iterator
    : public boost::iterator_facade<fibonacci_iterator, int, boost::bidirectional_traversal_tag, int> {

    explicit fibonacci_iterator(int const a = int(), int const b = int()) : a(a), b(b) {}

private:
    friend boost::iterator_core_access;

    int dereference() const { return a; }
    bool equal(fibonacci_iterator const& that) const { return a == that.a && b == that.b; }
    void increment() { std::tie(a, b) = nextFibonacci(a, b); }
    void decrement() { std::tie(a, b) = previousFibonacci(a, b); }
    int a;
    int b;
};

template<typename Predicate, typename Iterator>
struct take_while_iterator : public boost::iterator_adaptor<take_while_iterator<Predicate, Iterator>, Iterator> {\
    typedef boost::iterator_adaptor<take_while_iterator<Predicate, Iterator>, Iterator> super_t;

    take_while_iterator() {}
    take_while_iterator(Predicate predicate, Iterator x) : super_t(x), predicate(predicate) {}

private:
    friend boost::iterator_core_access;

    bool equal(take_while_iterator const& that) const { return !this->predicate(*this->base()); }

    Predicate predicate;
};

template <class Predicate, class Iterator>
take_while_iterator<Predicate,Iterator>
make_take_while_iterator(Predicate f, Iterator x) {
    return take_while_iterator<Predicate,Iterator>(f,x);
}

template< class P, class R >
struct take_while_range :
    boost::iterator_range<
        take_while_iterator< P,
            typename boost::range_iterator<R>::type
        >
    > {
private:
    typedef boost::iterator_range<
                take_while_iterator< P,
                    typename boost::range_iterator<R>::type
                >
            > base;

public:
    take_while_range( P p, R& r )
    : base( make_take_while_iterator( p, std::begin(r)),
            make_take_while_iterator( p, std::end(r)))
    { }
};

template< class T >
struct take_while_holder : boost::range_detail::holder<T> {
    take_while_holder( T r ) : holder<T>(r) { }
};

template< class InputRng, class Predicate >
inline take_while_range<Predicate, InputRng>
operator|( InputRng& r, const take_while_holder<Predicate>& f ) {
    return take_while_range<Predicate, InputRng>( f.val, r );
}

template< class InputRng, class Predicate >
inline take_while_range<Predicate, const InputRng>
operator|( const InputRng& r, const take_while_holder<Predicate>& f ) {
    return take_while_range<Predicate, const InputRng>( f.val, r );
}

const boost::range_detail::forwarder<take_while_holder> take_while = boost::range_detail::forwarder<take_while_holder>();

boost::iterator_range<fibonacci_iterator> fibonacci_range(int const a, int const b) {
    return boost::make_iterator_range(fibonacci_iterator(a, b), fibonacci_iterator());
}

struct IsEven {
    bool operator()(int const x) { return !(x & 1); }
} isEven;

static int filterImperative(int limit) {
    auto sum = 0;
    for(auto a = 1, b = 2; a < limit; std::tie(a, b) = nextFibonacci(a, b))
        if (isEven(a))
            sum += a;
    return sum;
}

std::tuple<int, int> nextEvenFibonacci(int const a, int const b) { return std::make_tuple(b, a + 4 * b); }
std::tuple<int, int> previousEvenFibonacci(int const a, int const b) { return std::make_tuple(b - 4 *a, a); }

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
    return boost::accumulate(fibonacci_range(1, 2)
                           | boost::adaptors::filtered(isEven)
                           | take_while(LessThanLimit(limit)), 0);
}

struct even_fibonacci_iterator
    : public boost::iterator_facade<even_fibonacci_iterator, int, boost::forward_traversal_tag, int> {

    explicit even_fibonacci_iterator(int const a = int(), int const b = int()) : a(a), b(b) {}

private:
    friend boost::iterator_core_access;

    int dereference() const { return a; }
    bool equal(even_fibonacci_iterator const& that) const { return a == that.a && b == that.b; }
    void increment() { std::tie(a, b) = nextEvenFibonacci(a, b); }
    void decrement() { std::tie(a, b) = previousEvenFibonacci(a, b); }
    int a;
    int b;
};

boost::iterator_range<even_fibonacci_iterator> even_fibonacci_range() {
    return boost::make_iterator_range(even_fibonacci_iterator(2, 8), even_fibonacci_iterator());
}

static int noFilterFunctional(int const limit) {
    return boost::accumulate(even_fibonacci_range() | take_while(LessThanLimit(limit)), 0);
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

    boost::for_each(boost::sort(results, byTime), [](Result const& result) {
        std::cout << std::setw(30) << std::left << result.name << " " << result.answer << " " <<
            std::fixed << std::setprecision(4) << result.time << "\n"; });
}
