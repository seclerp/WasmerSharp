using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
///  Support for surfacing .NET functions to the Wasm module.
/// </summary>
// This is WasmerImportFunc
public class ImportFunction : WasmerNativeHandle {
  internal ImportFunction (IntPtr handle) : base (handle) { }

  internal static WasmerValueType ValidateTypeToTag (Type type)
  {
    if (type.IsByRef)
      throw new ArgumentException ("The provided method can not out/ref parameters");

    if (type == typeof (int)) {
      return WasmerValueType.Int32;
    } else if (type == typeof (long)) {
      return WasmerValueType.Int64;
    } else if (type == typeof (double)) {
      return WasmerValueType.Float64;
    } else if (type == typeof (float)) {
      return WasmerValueType.Float32;
    } else
      throw new ArgumentException ("The method can only contain parameters of type int, long, float and double");
  }

  [DllImport (Library)]
  extern static IntPtr wasmer_import_func_new (IntPtr func, WasmerValueType [] pars, int paramLen, WasmerValueType [] returns, int retLen);

  /// <summary>
  ///    Creates an ImportFunction from a delegate method, the .NET method passed on the delegate will then be available to  be called by the Wasm runtime.
  /// </summary>
  /// <param name="method">The method to wrap.   The method can only contains int, long, float or double arguments.  The method return can include void, int, long, float and double. </param>

  public ImportFunction (Delegate method)
  {
    if (method == null)
      throw new ArgumentNullException (nameof (method));

    var methodInfo = method.Method;
    var methodPi = methodInfo.GetParameters ();
    if (methodPi.Length < 1)
      throw new ArgumentException ("The method must at least have one argument of type InstanceContext");

    var pars = new WasmerValueType [methodPi.Length-1];

    int i = 0;
    foreach (var pi in methodInfo.GetParameters ()) {
      var pt = pi.ParameterType;
      if (i == 0) {
        if (pt != typeof (InstanceContext)) {
          throw new ArgumentException ("The first method in the method must be of type InstanceContext");
        }
      } else {
        var vt = ValidateTypeToTag (pt);
        pars [i - 1] = vt;
      }
      i++;
    }

    WasmerValueType [] returnTag;

    var returnType = methodInfo.ReturnType;
    if (returnType == typeof (void)) {
      returnTag = new WasmerValueType [0];
    } else {
      returnTag = new WasmerValueType [1] { ValidateTypeToTag (returnType) };
    }

    handle = wasmer_import_func_new (
      Marshal.GetFunctionPointerForDelegate (method),
      pars, pars.Length, returnTag, returnTag.Length);
  }

  [DllImport (Library)]
  extern static void wasmer_import_func_destroy (IntPtr handle);

  internal override Action<IntPtr> GetHandleDisposer ()
  {
    return wasmer_import_func_destroy;
  }

  // I do not think these are necessary as these are just a way of getting the
  // data that is computed from the Delegate that creates the ImportFunc
  // wasmer_import_func_params
  // wasmer_import_func_params_arity
  // wasmer_import_func_returns
  // wasmer_import_func_returns_arity
}