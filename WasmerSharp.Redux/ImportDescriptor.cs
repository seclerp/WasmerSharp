namespace WasmerSharp;

/// <summary>
/// The import descriptors for a WebAssembly module describe the type of each import, iits name and the module name it belongs to.
/// </summary>
public class ImportDescriptor : WasmerNativeHandle {
  internal ImportDescriptor () { }

  /// <summary>
  /// Returns the descriptor kind
  /// </summary>
  public ImportExportKind Kind { get; internal set; }

  /// <summary>
  /// Gets module name for the import descriptor
  /// </summary>
  public string ModuleName { get; internal set; }

  /// <summary>
  /// Gets name for the import descriptor
  /// </summary>
  public string Name { get; internal set; }

  /// <summary>
  ///  Returns a human-readable description of the import
  /// </summary>
  public override string ToString ()
  {
    return $"{Kind}(\"{ModuleName}::{Name}\"";
  }
}