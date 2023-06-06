using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
/// Represents an ExportedFunction from WebAssembly to .NET
/// </summary>
public class ExportFunction : WasmerNativeHandle {
  internal bool owns;
  internal ExportFunction (IntPtr handle, bool owns) : base (handle)
  {
    this.owns = owns;
  }

  [DllImport (Library)]
  unsafe extern static WasmerResult wasmer_export_func_call (IntPtr handle, WasmerValue* values, int valueLen, WasmerValue* results, int resultLen);

  /// <summary>
  /// Calls the function with the specified parameters
  /// </summary>
  /// <param name="values">The values to pass to the exported function.</param>
  /// <param name="results">The array with the results, it should have enough space to hold all the results</param>
  /// <returns></returns>
  public bool Call (WasmerValue [] values, WasmerValue [] results)
  {
    if (values == null)
      throw new ArgumentNullException (nameof (values));
    if (results == null)
      throw new ArgumentNullException (nameof (results));

    unsafe {
      fixed (WasmerValue* v = &values [0]) {
        fixed (WasmerValue* result = &results [0]) {
          return wasmer_export_func_call (handle, v, values.Length, result, results.Length) != 0;
        }
      }
    }
  }

  [DllImport (Library)]
  extern static void wasmer_import_func_destroy (IntPtr handle);
  internal override Action<IntPtr> GetHandleDisposer ()
  {
    if (owns)
      return wasmer_import_func_destroy;
    else
      return null;
  }

  [DllImport (Library)]
  unsafe extern static WasmerResult wasmer_export_func_params (IntPtr handle, WasmerValueType* p, uint pLen);

  [DllImport (Library)]
  extern static WasmerResult wasmer_export_func_params_arity (IntPtr handle, out uint result);

  /// <summary>
  /// Returns the parameter types for the exported function as an array.   Returns null on error. You can use the LastError error property to get details on the error.
  /// </summary>
  public WasmerValueType [] Parameters {
    get {
      if (wasmer_export_func_params_arity (handle, out var npars) == WasmerResult.Error)
        return null;
      var tags = new WasmerValueType [npars];
      unsafe {
        fixed (WasmerValueType* t = &tags [0]) {
          if (wasmer_export_func_params (handle, t, npars) == WasmerResult.Ok)
            return tags;
        }
      }
      return null;
    }
  }

  [DllImport (Library)]
  unsafe extern static WasmerResult wasmer_export_func_returns (IntPtr handle, WasmerValueType* ret, uint retLen);

  [DllImport (Library)]
  extern static WasmerResult wasmer_export_func_returns_arity (IntPtr handle, out uint result);

  /// <summary>
  /// Returns the return types for the exported function as an array.   Returns null on error. You can use the LastError error property to get details on the error.
  /// </summary>
  public WasmerValueType [] Returns {
    get {
      if (wasmer_export_func_returns_arity (handle, out var npars) == WasmerResult.Error)
        return null;
      var tags = new WasmerValueType [npars];
      unsafe {
        fixed (WasmerValueType* t = &tags [0]) {
          if (wasmer_export_func_returns (handle, t, npars) == WasmerResult.Ok)
            return tags;
        }
      }
      return null;
    }
  }
}