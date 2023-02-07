using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jds.DependencyInjection.AsyncExtras;

/// <summary>
///   Methods registering async services.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  ///   Adds a <see cref="ServiceDescriptor" /> for an <see cref="IAsyncResolver{T}" /> of <typeparamref name="TService" />
  ///   if that type has not been registered.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     When requiring this async dependency (<typeparamref name="TService" />) via constructor parameters, make sure to
  ///     use an <see cref="IAsyncResolver{T}" /> of <typeparamref name="TService" />.
  ///   </para>
  ///   <para>
  ///     This registration does not enable <typeparamref name="TService" /> to be resolved from
  ///     <see cref="IServiceProvider" /> instances created from this <see cref="IServiceCollection" />.
  ///   </para>
  ///   <para>
  ///     Alternately, use the <see cref="ServiceProviderAsyncExtensions.GetServiceAsync{T}" /> and
  ///     <see cref="ServiceProviderAsyncExtensions.GetRequiredServiceAsync{T}" /> methods to access
  ///     <typeparamref name="TService" /> (from any <see cref="IServiceProvider" /> instances created from this
  ///     <see cref="IServiceCollection" />).
  ///   </para>
  /// </remarks>
  /// <param name="serviceCollection">This <see cref="IServiceCollection" /> instance.</param>
  /// <param name="asyncFactory">A factory method which asynchronously creates <typeparamref name="TService" /> instances.</param>
  /// <param name="lifetime">A <see cref="ServiceLifetime" />.</param>
  /// <typeparam name="TService">The type of the service to add.</typeparam>
  /// <returns>This <see cref="IServiceCollection" />.</returns>
  public static IServiceCollection TryAddAsyncResolver<TService>(
    this IServiceCollection serviceCollection,
    Func<IServiceProvider, Task<TService>> asyncFactory,
    ServiceLifetime lifetime = ServiceLifetime.Transient
  )
  {
    serviceCollection.TryAdd(
      new ServiceDescriptor(
        typeof(IAsyncResolver<TService>),
        serviceProvider => new AsyncResolver<TService>(serviceProvider, asyncFactory),
        lifetime
      )
    );
    return serviceCollection;
  }
}
