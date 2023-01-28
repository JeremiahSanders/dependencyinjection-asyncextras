using Jds.LanguageExt.Extras;
using Jds.TestingUtils.Randomization;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;

namespace Jds.DependencyInjectionAsyncExtras.Tests.Unit;

public static class AsyncServiceResolverTests
{
  private static IAsyncResolver<T> CreateProvider<T>(
    Func<IServiceCollection, IServiceCollection> arrangeServices, Func<IServiceProvider, Task<T>> resolver)
  {
    var serviceProvider = arrangeServices(new ServiceCollection())
      .BuildServiceProvider(new ServiceProviderOptions
        {ValidateOnBuild = true, ValidateScopes = true});
    return new AsyncResolver<T>(serviceProvider, resolver);
  }

  public class GetValueAsyncTests
  {
    [Fact]
    public async Task ReturnsExpectedValue()
    {
      var expected = new SimpleReference
      {
        ReferenceType = Randomizer.Shared.RandomStringLatin(10)
      };
      var provider = CreateProvider(Prelude.identity, AsyncFactoryBuilders.FromDelayedConstant(expected));

      var actual = await provider.GetValueAsync();

      Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(15)]
    public async Task CanReattemptAsyncFailures(int count)
    {
      var expected = new SimpleReference
      {
        ReferenceType = Randomizer.Shared.RandomStringLatin(10)
      };
      var regenerateIfFaulted = true;
      var expectedFailureAttempts = count - 1;
      var provider = CreateProvider(Prelude.identity,
        AsyncFactoryBuilders.Eventual(expected, Math.Clamp(expectedFailureAttempts, 0, 1000))
      );
      var failures = 0;
      SimpleReference? lastSuccess = null;

      for (var i = 0; i < count; i++)
        (await Prelude.TryAsync(() => provider.GetValueAsync(regenerateIfFaulted)).Try())
          .Tap(value => { lastSuccess = value; }, _ => failures++);

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
        ReferenceType = Randomizer.Shared.RandomStringLatin(10)
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
        (await Prelude.TryAsync(() => provider.GetValueAsync(regenerateIfFaulted, regenerateIfCanceled)).Try())
          .Tap(successes.Add, exceptions.Add);

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
        ReferenceType = Randomizer.Shared.RandomStringLatin(10)
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
        (await Prelude.TryAsync(() => provider.GetValueAsync(regenerateIfFaulted, regenerateIfCanceled)).Try())
          .Tap(successes.Add, exceptions.Add);

      Assert.Equal(attemptCount, exceptions.Count);
      Assert.Empty(successes);
    }
  }
}
