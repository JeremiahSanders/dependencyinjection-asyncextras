namespace Jds.DependencyInjection.AsyncExtras;

/// <summary>
///   Default implementation of <see cref="IAsyncResolver{T}" />.
/// </summary>
/// <typeparam name="T"></typeparam>
public class AsyncResolver<T> : IAsyncResolver<T>
{
  private readonly Func<IServiceProvider, Task<T>> _factory;
  private readonly IServiceProvider _serviceProvider;
  private Lazy<Task<T>> _lazy;

  /// <summary>
  ///   Initializes a new instance of the <see cref="AsyncResolver{T}" /> class.
  /// </summary>
  /// <param name="serviceProvider">A service provider.</param>
  /// <param name="asyncFactory">A factory method which asynchronously creates <typeparamref name="T" /> instances.</param>
  public AsyncResolver(IServiceProvider serviceProvider, Func<IServiceProvider, Task<T>> asyncFactory)
  {
    _serviceProvider = serviceProvider;
    _factory = asyncFactory;
    _lazy = CreateLazy();
  }

  /// <inheritdoc cref="IAsyncResolver{T}.GetValueAsync" />
  public Task<T> GetValueAsync(
    bool regenerateIfFaulted = true,
    bool regenerateIfCanceled = true,
    bool ignoreCache = false
  )
  {
    if ((!ignoreCache && regenerateIfFaulted && _lazy is {IsValueCreated: true, Value.IsFaulted: true}) ||
        (!ignoreCache && regenerateIfCanceled && _lazy is {IsValueCreated: true, Value.IsCanceled: true}))
    {
      _lazy = CreateLazy();
    }

    return ignoreCache ? _factory(_serviceProvider) : _lazy.Value;
  }

  private Lazy<Task<T>> CreateLazy()
  {
    return new Lazy<Task<T>>(() => _factory(_serviceProvider));
  }
}
