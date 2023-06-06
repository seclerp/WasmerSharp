namespace WasmerSharp;

/// <summary>
/// Describes the types exposed by the WasmerBridge
/// </summary>
public enum WasmerValueType : uint {
  /// <summary>
  /// The type is 32-bit integer
  /// </summary>
  Int32,
  /// <summary>
  /// The type is a 64 bit integer
  /// </summary>
  Int64,
  /// <summary>
  /// The type is a 32-bit floating point
  /// </summary>
  Float32,
  /// <summary>
  /// The type is a 64-bit floating point
  /// </summary>
  Float64
}