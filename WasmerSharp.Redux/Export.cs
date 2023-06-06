using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
/// Represents an exported object from a Wasm Instance
/// </summary>
/// <remarks>
/// <para>
///   A module can declare a sequence of exports which are returned at
///   instantiation time to the host environment.
/// </para>
/// <para>
///    Exports have a name, which is required to be valid UTF-8, whose meaning is defined by the host environment, a type, indicating whether the export is a function, global, memory or table.
/// </para>
/// </remarks>
public class Export : WasmerNativeHandle {
  internal Export (IntPtr handle) : base (handle) { }

  [DllImport (Library)]
  static extern ImportExportKind wasmer_export_kind (IntPtr handle);

  /// <summary>
  /// Gets the kind for the exported item
  /// </summary>
  public ImportExportKind Kind => wasmer_export_kind (handle);

  [DllImport (Library)]
  static extern WasmerByteArray wasmer_export_name (IntPtr handle);

  /// <summary>
  /// Gets the name for the export
  /// </summary>
  public string Name => wasmer_export_name (handle).ToString ();

  [DllImport (Library)]
  extern static IntPtr wasmer_export_to_func (IntPtr handle);

  /// <summary>
  /// Gets the exported function
  /// </summary>
  /// <returns>Null on error, or the exported function.  You can use the LastError error property to get details on the error.</returns>
  public ExportFunction GetExportFunction ()
  {
    var rh = wasmer_export_to_func (handle);
    if (rh != IntPtr.Zero)
      return new ExportFunction (rh, owns: false);
    return null;
  }

  [DllImport (Library)]
  extern static WasmerResult wasmer_export_to_memory (IntPtr handle, out IntPtr memory);

  /// <summary>
  /// Returns the memory object from the export
  /// </summary>
  /// <returns></returns>
  public Memory GetMemory ()
  {
    // DO WE OWN THE MEM HANDLE?
    if (wasmer_export_to_memory (handle, out var mem) == WasmerResult.Error)
      return null;
    return new Memory (mem);
  }

  [DllImport (Library)]
  extern static void wasmer_exports_destroy (IntPtr handle);

  internal override Action<IntPtr> GetHandleDisposer ()
  {
    return wasmer_exports_destroy;
  }
}