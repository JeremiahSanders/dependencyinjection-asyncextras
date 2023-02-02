using LanguageExt;

namespace Jds.DependencyInjection.AsyncExtras.Tests.Unit;

internal static class AsyncFactoryBuilders
{
  private static Exception CreateException()
  {
    return new Exception("Arranged failure");
  }

  public static Func<IServiceProvider, Task<T>> Cancellations<T>(T value, int cancellationCount,
    int millisecondDelay = 1
  )
  {
    var failures = 0;

    Task<T> DelayedFailure(IServiceProvider _)
    {
      if (failures >= cancellationCount)
      {
        return value.AsTask();
      }

      failures++;
      return new Task<T>(() => throw new InvalidOperationException("Shouldn't execute"), new CancellationToken(true));
    }

    return DelayedFailure;
  }

  public static Func<IServiceProvider, Task<T>> Eventual<T>(T value, int failureCount, int millisecondDelay = 1)
  {
    var failures = 0;

    async Task<T> DelayedFailure(IServiceProvider _)
    {
      await Task.Delay(millisecondDelay);

      if (failures >= failureCount)
      {
        return value;
      }

      failures++;
      throw CreateException();
    }

    return DelayedFailure;
  }

  public static Func<IServiceProvider, Task<T>> AlwaysFails<T>(int millisecondDelay = 1)
  {
    async Task<T> DelayedFailure(IServiceProvider _)
    {
      await Task.Delay(millisecondDelay);
      throw CreateException();
    }

    return DelayedFailure;
  }

  public static Func<IServiceProvider, Task<T>> FromDelayedConstant<T>(T value, int millisecondDelay = 1)
  {
    async Task<T> DelayedConstant(IServiceProvider _)
    {
      await Task.Delay(millisecondDelay);
      return value;
    }

    return DelayedConstant;
  }
}
