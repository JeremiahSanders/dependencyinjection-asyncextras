namespace Jds.DependencyInjectionAsyncExtras.Tests.Unit;

/// <summary>
///   Simple reference type usable for generic type arguments.
/// </summary>
internal class SimpleReference
{
  public SimpleReference? Child { get; set; }

  public string? NullableReferenceType { get; set; }
  public int? NullableValueType { get; set; }
  public string ReferenceType { get; set; } = string.Empty;
  public int ValueType { get; set; }
}
