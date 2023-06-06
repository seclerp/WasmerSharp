namespace WasmerSharp;

/// <summary>
/// Describes the kind of export or import
/// </summary>
public enum ImportExportKind : uint {
  /// <summary>
  /// The import or export is a Function
  /// </summary>
  Function,
  /// <summary>
  /// The import or export is a global
  /// </summary>
  Global,
  /// <summary>
  ///  The import or export is a memory object
  /// </summary>
  Memory,
  /// <summary>
  /// The import or export is a table
  /// </summary>
  Table
}