namespace WasmerSharp;

/// <summary>
/// Represents an export from a web assembly module
/// </summary>
public class ExportDescriptor {
  /// <summary>
  ///  Gets export descriptor kind
  /// </summary>
  public ImportExportKind Kind { get; internal set; }


  /// <summary>
  /// Gets name for the export descriptor
  /// </summary>
  public string Name { get; internal set; }

  /// <summary>
  ///  Returns a human-readable description of the export
  /// </summary>
  public override string ToString ()
  {
    return $"{Kind} (\"{Name}\")";
  }
}