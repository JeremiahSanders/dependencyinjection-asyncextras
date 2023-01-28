using Microsoft.Extensions.DependencyInjection;

namespace Jds.DependencyInjectionAsyncExtras;

/// <summary>
///   Methods adding async resolution to <see cref="IServiceProvider" />.
/// </summary>
public static class ServiceProviderAsyncExtensions
{
  /// <summary>
  ///   Get service of type <typeparamref name="T" /> from the <see cref="IServiceProvider" />.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     If <typeparamref name="T" /> has an <see cref="IAsyncResolver{T}" /> registered, it will be resolved and the
  ///     result of its <see cref="IAsyncResolver{T}.GetValueAsync" /> will be returned.
  ///   </para>
  /// </remarks>
  /// <typeparam name="T">The type of service object to get.</typeparam>
  /// <param name="serviceProvider">The <see cref="IServiceProvider" /> to retrieve the service object from.</param>
  /// <returns>A service object of type <typeparamref name="T" /> or null if there is no such service.</returns>
  public static async Task<T?> GetServiceAsync<T>(this IServiceProvider serviceProvider)
  {
    var asyncProvider = serviceProvider.GetService<IAsyncResolver<T>>();
    return asyncProvider != null
      ? await asyncProvider.GetValueAsync()
      : serviceProvider.GetService<T>();
  }

  /// <inheritdoc cref="GetServiceAsync{T}" />
  /// <exception cref="System.InvalidOperationException">
  ///   There is no service of type <typeparamref name="T" /> or
  ///   <see cref="IAsyncResolver{T}" />.
  /// </exception>
  public static async Task<T> GetRequiredServiceAsync<T>(this IServiceProvider serviceProvider) where T : notnull
  {
    var asyncProvider = serviceProvider.GetService<IAsyncResolver<T>>();
    return asyncProvider != null
      ? await asyncProvider.GetValueAsync()
      : serviceProvider.GetRequiredService<T>();
  }
}
