namespace Jds.DependencyInjection.AsyncExtras;

/// <summary>
///   A provider of asynchronous <typeparamref name="T" /> values.
/// </summary>
/// <remarks>
///   This type is intended for use with a <see cref="IServiceProvider" />. Specifically, it is meant to address resolving
///   dependency injection services which must be accessed asynchronously. E.g., a configuration object which must be
///   requested from a network source.
/// </remarks>
/// <typeparam name="T">A type which is resolved asynchronously.</typeparam>
public interface IAsyncResolver<T>
{
  /// <summary>
  ///   Get the asynchronous <typeparamref name="T" />.
  /// </summary>
  /// <remarks>
  ///   <para>Value resolved asynchronously is cached by default.</para>
  /// </remarks>
  /// <param name="regenerateIfFaulted">
  ///   A value indicating that if the cached <see cref="Task{TResult}" /> <see cref="Task.IsFaulted" />,
  ///   reattempt the asynchronous resolution.
  /// </param>
  /// <param name="regenerateIfCanceled">
  ///   A value indicating that if the cached <see cref="Task{TResult}" /> <see cref="Task.IsCanceled" />,
  ///   reattempt the asynchronous resolution.
  /// </param>
  /// <param name="ignoreCache">
  ///   A value indicating that a new asynchronous resolution should be performed, even if a cached
  ///   <see cref="Task{TResult}" /> exists.
  /// </param>
  /// <returns>A <see cref="Task{TResult}" />.</returns>
  Task<T> GetValueAsync(bool regenerateIfFaulted = true, bool regenerateIfCanceled = true, bool ignoreCache = false);
}
