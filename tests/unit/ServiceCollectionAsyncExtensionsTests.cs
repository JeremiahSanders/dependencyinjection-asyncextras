using Microsoft.Extensions.DependencyInjection;

namespace Jds.DependencyInjection.AsyncExtras.Tests.Unit;

public static class ServiceCollectionExtensionsTests
{
  internal static IServiceProvider ArrangeServiceProvider<T>(Func<IServiceProvider, Task<T>> resolver,
    ServiceLifetime lifetime)
  {
    var serviceCollection = new ServiceCollection();
    serviceCollection.TryAddAsyncResolver(resolver, lifetime);
    return serviceCollection.BuildServiceProvider(true);
  }

  public class TryAddAsyncResolverTests
  {
    [Fact]
    public async Task ValueTypes()
    {
      const int millisecondDelay = 3;
      const int expectedValue = 42;


      var asyncService = await RegisterAndResolve(
        AsyncFactoryBuilders.FromDelayedConstant(expectedValue, millisecondDelay)
      );

      Assert.Equal(expectedValue, asyncService);
    }

    [Fact]
    public async Task ReferenceTypes()
    {
      const int millisecondDelay = 2;
      var expectedValue = new SimpleReference
      {
        ReferenceType = Guid.NewGuid().ToString("D"),
        Child = new SimpleReference
        {
          ReferenceType = Guid.NewGuid().ToString("D")
        }
      };
      var asyncService = await RegisterAndResolve(
        AsyncFactoryBuilders.FromDelayedConstant(expectedValue, millisecondDelay)
      );

      Assert.Equal(expectedValue, asyncService);
    }


    private static Task<T> RegisterAndResolve<T>(Func<IServiceProvider, Task<T>> resolver) where T : notnull
    {
      return ArrangeServiceProvider(resolver, ServiceLifetime.Singleton).GetRequiredServiceAsync<T>();
    }
  }
}
