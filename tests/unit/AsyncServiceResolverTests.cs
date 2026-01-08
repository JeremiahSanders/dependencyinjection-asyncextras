using System.Collections.Concurrent;

using Jds.LanguageExt.Extras;
using Jds.TestingUtils.Randomization;

using LanguageExt;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.DependencyInjection.AsyncExtras.Tests.Unit;

public static class AsyncServiceResolverTests
{
  private static IAsyncResolver<T> CreateProvider<T>(
    Func<IServiceCollection, IServiceCollection> arrangeServices, Func<IServiceProvider, Task<T>> resolver
  )
  {
    ServiceProvider serviceProvider = arrangeServices(new ServiceCollection())
      .BuildServiceProvider(new ServiceProviderOptions
        { ValidateOnBuild = true, ValidateScopes = true });
    return new AsyncResolver<T>(serviceProvider, resolver);
  }


  /// <summary>
  ///   Create a cancellation token source which will automatically cancel.
  /// </summary>
  /// <param name="timeoutInMs"></param>
  /// <returns></returns>
  private static CancellationTokenSource CreateAutoCancelingSource(long timeoutInMs = 2_000)
  {
    return CreateAutoCancelingSource(TimeSpan.FromMilliseconds(timeoutInMs));
  }

  /// <summary>
  ///   Create a cancellation token source which will automatically cancel.
  /// </summary>
  /// <param name="timeout"></param>
  /// <returns></returns>
  private static CancellationTokenSource CreateAutoCancelingSource(TimeSpan timeout)
  {
    return new CancellationTokenSource(timeout);
  }

  public class GetValueAsyncTests
  {
    [Fact]
    public async Task ReturnsExpectedValue()
    {
      var expected = new SimpleReference
      {
        ReferenceType = Randomizer.Shared.RandomStringLatin(10),
      };
      var provider = CreateProvider(Prelude.identity, AsyncFactoryBuilders.FromDelayedConstant(expected));

      SimpleReference actual = await provider.GetValueAsync();

      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task InitializationIsThreadSafe_TaskWhenAll()
    {
      CancellationTokenSource threadHangPreventionSource = CreateAutoCancelingSource();
      const int parallelRequests = 100;
      ConcurrentBag<DateTimeOffset> initializations = [];
      ConcurrentBag<SimpleReference> results = [];
      var provider = CreateProvider(Prelude.identity, async serviceProvider =>
      {
        initializations.Add(DateTimeOffset.UtcNow);
        var expected = new SimpleReference
        {
          ReferenceType = Randomizer.Shared.RandomStringLatin(10),
        };
        results.Add(expected);

        await Task.Delay(100, threadHangPreventionSource.Token);

        return expected;
      });

      var requests = Enumerable.Range(0, parallelRequests).Select(async _ =>
      {
        threadHangPreventionSource.Token.ThrowIfCancellationRequested();
        return await provider.GetValueAsync();
      }).ToList();
      var values = await Task.WhenAll(requests);

      Assert.Equal(initializations.Count, results.Count);
      Assert.Single(initializations);
      Assert.Equal(parallelRequests, values.Length);
    }

    [Fact]
    public async Task InitializationIsThreadSafe_FailuresAreThreadSafe()
    {
      CancellationTokenSource threadHangPreventionSource = CreateAutoCancelingSource(TimeSpan.FromSeconds(2));
      const int maxParallelism = 8;
      // each "batch" of parallel requests will receive the same value
      const int failuresBeforeItWorks = maxParallelism + 1;
      const int totalRequests = failuresBeforeItWorks * 20;

      ConcurrentBag<int> invokedValues = [];
      ConcurrentBag<string> generatedValues = [];
      ConcurrentStack<string> activeRequests = [];

      var provider = CreateProvider(Prelude.identity, async serviceProvider =>
      {
        if (!activeRequests.IsEmpty)
        {
          throw new InvalidOperationException(
            $"Parallel initialization detected! The active requests must be empty. Count: {activeRequests.Count}");
        }

        string value = Randomizer.Shared.RandomStringLatin(16);
        var expected = new SimpleReference
        {
          ReferenceType = value,
        };
        generatedValues.Add(value);
        activeRequests.Push(value);

        await Task.Delay(25, threadHangPreventionSource.Token);

        if (generatedValues.Count < failuresBeforeItWorks)
        {
          activeRequests.TryPop(out _);
          throw new InvalidOperationException(
            $"Planned failure. Current initialization count: {generatedValues.Count}");
        }

        activeRequests.TryPop(out string? topRequest);
        if (topRequest != expected.ReferenceType)
        {
          throw new InvalidOperationException(
            "Parallel initialization detected! Top resolution request is not the active request.");
        }

        if (!activeRequests.IsEmpty)
        {
          throw new InvalidOperationException(
            $"Parallel initialization detected! The resolution requests is expected to be empty at end of initialization. Count: {activeRequests.Count}");
        }

        return expected;
      });

      ConcurrentBag<SimpleReference> responses = [];
      await Parallel.ForAsync(0, totalRequests,
        new ParallelOptions
          { MaxDegreeOfParallelism = maxParallelism, CancellationToken = threadHangPreventionSource.Token },
        async (item, token) =>
        {
          invokedValues.Add(item);
          try
          {
            SimpleReference result = await provider.GetValueAsync().WaitAsync(token);

            responses.Add(result);
          }
          catch (InvalidOperationException e) when (e.Message.StartsWith("Planned failure."))
          {
            // No op - planned failure
            e = e;
          }
        }
      );

      Assert.False(threadHangPreventionSource.IsCancellationRequested, "Initialization timed out.");
      Assert.Equal(totalRequests, invokedValues.Count);
      Assert.NotEmpty(responses);
      Assert.All(responses, sr => Assert.Contains(generatedValues, gv => gv == sr.ReferenceType));
      Assert.Equal(failuresBeforeItWorks, generatedValues.Count);
    }

    [Fact]
    public async Task InitializationIsThreadSafe_ParallelFor()
    {
      CancellationTokenSource threadHangPreventionSource = CreateAutoCancelingSource();
      const int parallelRequests = 100;
      ConcurrentBag<DateTimeOffset> initializations = [];
      ConcurrentBag<SimpleReference> results = [];

      var provider = CreateProvider(Prelude.identity, async serviceProvider =>
      {
        initializations.Add(DateTimeOffset.UtcNow);
        var expected = new SimpleReference
        {
          ReferenceType = Randomizer.Shared.RandomStringLatin(10),
        };
        results.Add(expected);

        await Task.Delay(100, threadHangPreventionSource.Token);

        return expected;
      });

      ConcurrentBag<SimpleReference> responses = [];
      await Parallel.ForAsync(0, parallelRequests,
        new ParallelOptions { MaxDegreeOfParallelism = 12, CancellationToken = threadHangPreventionSource.Token },
        async (_, _) =>
        {
          SimpleReference result = await provider.GetValueAsync();

          responses.Add(result);
        }
      );

      Assert.Equal(initializations.Count, results.Count);
      Assert.Single(initializations);
      SimpleReference expected = results.Single();
      Assert.Equal(parallelRequests, responses.Count);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(15)]
    public async Task CanReattemptAsyncFailures(int count)
    {
      var expected = new SimpleReference
      {
        ReferenceType = Randomizer.Shared.RandomStringLatin(10),
      };
      var regenerateIfFaulted = true;
      int expectedFailureAttempts = count - 1;
      var provider = CreateProvider(Prelude.identity,
        AsyncFactoryBuilders.Eventual(expected, Math.Clamp(expectedFailureAttempts, 0, 1000))
      );
      var failures = 0;
      SimpleReference? lastSuccess = null;

      for (var i = 0; i < count; i++)
      {
        (await Prelude.TryAsync(() => provider.GetValueAsync(regenerateIfFaulted)).Try())
          .Tap(value => { lastSuccess = value; }, _ => failures++);
      }

      Assert.Equal(expectedFailureAttempts, failures);
      Assert.Equal(expected, lastSuccess);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(13)]
    public async Task CachesFailuresIfNotRegenerated(int attemptCount)
    {
      var expected = new SimpleReference
      {
        ReferenceType = Randomizer.Shared.RandomStringLatin(10),
      };
      var regenerateIfFaulted = false;
      var regenerateIfCanceled = true; // To ensure we're testing faults not cancellations
      var requiredFailures = 1;
      var provider = CreateProvider(Prelude.identity,
        AsyncFactoryBuilders.Eventual(expected, requiredFailures)
      );

      List<SimpleReference> successes = new();
      List<Exception> exceptions = new();

      for (var i = 0; i < attemptCount; i++)
      {
        (await Prelude.TryAsync(() => provider.GetValueAsync(regenerateIfFaulted, regenerateIfCanceled)).Try())
          .Tap(successes.Add, exceptions.Add);
      }

      Assert.Equal(attemptCount, exceptions.Count);
      Assert.Empty(successes);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(13)]
    public async Task CachesCancellationsIfNotRegenerated(int attemptCount)
    {
      var expected = new SimpleReference
      {
        ReferenceType = Randomizer.Shared.RandomStringLatin(10),
      };
      var regenerateIfFaulted = true; // To ensure we're testing cancellations not faults
      var regenerateIfCanceled = false;
      var requiredCancellations = 1;
      var provider = CreateProvider(Prelude.identity,
        AsyncFactoryBuilders.Cancellations(expected, requiredCancellations)
      );

      List<SimpleReference> successes = new();
      List<Exception> exceptions = new();

      for (var i = 0; i < attemptCount; i++)
      {
        (await Prelude.TryAsync(() => provider.GetValueAsync(regenerateIfFaulted, regenerateIfCanceled)).Try())
          .Tap(successes.Add, exceptions.Add);
      }

      Assert.Equal(attemptCount, exceptions.Count);
      Assert.Empty(successes);
    }
  }
}
