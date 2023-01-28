using Microsoft.Extensions.DependencyInjection;

namespace Jds.DependencyInjection.AsyncExtras.Tests.Unit;

public static class ServiceProviderAsyncExtensions
{
  public class GetRequiredServiceAsyncTests
  {
    [Fact]
    public async Task GivenRegisteredProviderReturnsExpectedService()
    {
      var provider =
        ServiceCollectionExtensionsTests.ArrangeServiceProvider(
          AsyncFactoryBuilders.FromDelayedConstant(new SimpleReference()), ServiceLifetime.Singleton);

      var asyncProvider = await provider.GetRequiredServiceAsync<SimpleReference>();

      Assert.NotNull(asyncProvider);
    }

    [Fact]
    public async Task GivenUnregisteredProviderThrowsException()
    {
      var provider =
        ServiceCollectionExtensionsTests.ArrangeServiceProvider(
          AsyncFactoryBuilders.FromDelayedConstant(new SimpleReference()), ServiceLifetime.Singleton);

      await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetRequiredServiceAsync<int>());
    }
  }

  public class GetServiceAsyncTests
  {
    [Fact]
    public async Task GivenRegisteredProviderReturnsExpectedService()
    {
      var provider =
        ServiceCollectionExtensionsTests.ArrangeServiceProvider(
          AsyncFactoryBuilders.FromDelayedConstant(new SimpleReference()), ServiceLifetime.Singleton);

      var asyncProvider = await provider.GetServiceAsync<SimpleReference>();

      Assert.NotNull(asyncProvider);
    }

    [Fact]
    public async Task GivenUnregisteredProviderReturnsNull()
    {
      var provider =
        ServiceCollectionExtensionsTests.ArrangeServiceProvider(
          AsyncFactoryBuilders.FromDelayedConstant(42), ServiceLifetime.Singleton);

      var possibleProvider = await provider.GetServiceAsync<SimpleReference>();

      Assert.Null(possibleProvider);
    }
  }
}
