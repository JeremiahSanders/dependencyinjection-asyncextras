# ServiceProviderAsyncExtensions.GetServiceAsync&lt;T&gt; method

Get service of type *T* from the IServiceProvider.

```csharp
public static Task<T?> GetServiceAsync<T>(this IServiceProvider serviceProvider)
```

| parameter | description |
| --- | --- |
| T | The type of service object to get. |
| serviceProvider | The IServiceProvider to retrieve the service object from. |

## Return Value

A service object of type *T* or null if there is no such service.

## Remarks

If *T* has an [`IAsyncResolver`](../IAsyncResolver-1.md) registered, it will be resolved and the result of its [`GetValueAsync`](../IAsyncResolver-1/GetValueAsync.md) will be returned.

## See Also

* class [ServiceProviderAsyncExtensions](../ServiceProviderAsyncExtensions.md)
* namespace [Jds.DependencyInjection.AsyncExtras](../../DependencyInjection.AsyncExtras.md)

<!-- DO NOT EDIT: generated by xmldocmd for DependencyInjection.AsyncExtras.dll -->
