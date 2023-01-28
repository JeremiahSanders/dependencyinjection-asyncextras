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
    if ((!ignoreCache && regenerateIfFaulted && _lazy.IsValueCreated && _lazy.Value.IsFaulted) ||
        (regenerateIfCanceled && _lazy.IsValueCreated && _lazy.Value.IsCanceled)) ResetLazy();
    return ignoreCache ? _factory(_serviceProvider) : _lazy.Value;
  }

  private Lazy<Task<T>> CreateLazy()
  {
    return new Lazy<Task<T>>(() => _factory(_serviceProvider));
  }

  private Lazy<Task<T>> ResetLazy()
  {
    _lazy = CreateLazy();
    return _lazy;
  }
}
