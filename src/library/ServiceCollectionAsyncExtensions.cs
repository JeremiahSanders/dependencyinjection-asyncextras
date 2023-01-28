using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jds.DependencyInjection.AsyncExtras;

/// <summary>
///   Methods registering async services.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  ///   Adds a <see cref="ServiceDescriptor" /> for an <see cref="IAsyncResolver{T}" /> of <typeparamref name="T" /> if that
  ///   type has not been registered.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     When requiring this async dependency (<typeparamref name="T" />) via constructor parameters, make sure to use an
  ///     <see cref="IAsyncResolver{T}" /> of <typeparamref name="T" />.
  ///   </para>
  ///   <para>
  ///     This registration does not enable <typeparamref name="T" /> to be resolved from <see cref="IServiceProvider" />
  ///     instances created from this <see cref="IServiceCollection" />.
  ///   </para>
  ///   <para>
  ///     Alternately, use the <see cref="ServiceProviderAsyncExtensions.GetServiceAsync{T}" /> and
  ///     <see cref="ServiceProviderAsyncExtensions.GetRequiredServiceAsync{T}" /> methods to access
  ///     <typeparamref name="T" /> (from any <see cref="IServiceProvider" /> instances created from this
  ///     <see cref="IServiceCollection" />).
  ///   </para>
  /// </remarks>
  /// <param name="serviceCollection"></param>
  /// <param name="asyncFactory"></param>
  /// <param name="lifetime"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public static IServiceCollection TryAddAsyncResolver<T>(
    this IServiceCollection serviceCollection,
    Func<IServiceProvider, Task<T>> asyncFactory,
    ServiceLifetime lifetime = ServiceLifetime.Transient
  )
  {
    serviceCollection.TryAdd(
      new ServiceDescriptor(
        typeof(IAsyncResolver<T>),
        serviceProvider => new AsyncResolver<T>(serviceProvider, asyncFactory),
        lifetime
      )
    );
    return serviceCollection;
  }
}
