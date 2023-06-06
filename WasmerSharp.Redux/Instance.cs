using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
/// Instances represents all the state associated with a module.   These are created by calling Module.Instantiate or by calling the Instance constructor.
/// </summary>
/// <remarks>
/// At runtime, a module can be instantiated with a set of import values to produce an instance, which is an
/// immutable tuple referencing all the state accessible to the running module. Multiple module instances
/// can access the same shared state which is the basis for dynamic linking in WebAssembly.
/// </remarks>
public class Instance : WasmerNativeHandle {
  internal Instance (IntPtr handle) : base (handle) { }

  [DllImport (Library)]
  unsafe extern static WasmerResult wasmer_instance_call (IntPtr handle, string name, WasmerValue* par, uint parLen, WasmerValue* res, uint resLen);

  [DllImport (Library)]
  unsafe extern static WasmerResult wasmer_instantiate (out IntPtr handle, IntPtr buffer, uint len, wasmer_import* imports, int imports_len);

  /// <summary>
  /// Creates a new Instance from the given wasm bytes and imports. 
  /// </summary>
  /// <param name="wasm">Wasm byte code</param>
  /// <param name="imports">The list of imports to pass, usually Function, Global and Memory</param>
  /// <returns>A Wasmer.Instance on success, or null on error.   You can use the LastError error property to get details on the error.</returns>
  public Instance (byte [] wasm, params Import [] imports)
  {
    if (wasm == null)
      throw new ArgumentNullException (nameof (wasm));
    if (imports == null)
      throw new ArgumentNullException (nameof (imports));

    var llimports = new wasmer_import [imports.Length];
    for (int i = 0; i < imports.Length; i++) {
      llimports [i].import_name = WasmerByteArray.FromString (imports [i].ImportName);
      llimports [i].module_name = WasmerByteArray.FromString (imports [i].ModuleName);
      llimports [i].tag = imports [i].Kind;
      llimports [i].value = imports [i].payload.handle;
    }
    unsafe {
      fixed (wasmer_import* p = &llimports [0]) {
        fixed (byte* bp = &wasm [0]) {
          if (wasmer_instantiate (out var result, (IntPtr)bp, (uint)wasm.Length, p, llimports.Length) == WasmerResult.Ok)
            handle = result;
          else
            throw new Exception ("Error instantiating from the provided wasm file" + LastError);
        }
      }
    }
  }
  /// <summary>
  /// Calls the specified function with the provided arguments
  /// </summary>
  /// <param name="functionName">Namer of the exported function to call in the instane</param>
  /// <param name="parameters">The parameters to pass to the function</param>
  /// <param name="results">The array where the return values are returned</param>
  /// <returns>True on success, false on failure</returns>
  public bool Call (string functionName, WasmerValue [] parameters, WasmerValue [] results)
  {
    unsafe {
      uint plen = (uint) parameters.Length;

      // The API does not like to get a null value, so we need to pass a pointer to something
      // and a length of zero.
      if (plen == 0) {
        parameters = new WasmerValue [1];
        parameters [0] = 0;
      }

      fixed (WasmerValue* p = &parameters [0]) {
        fixed (WasmerValue* r = &results [0]) {
          return wasmer_instance_call (handle, functionName, p, plen, r, (uint)results.Length) == WasmerResult.Ok;
        }
      }
    }
  }

  /// <summary>
  /// Calls the specified function with the provided arguments
  /// </summary>
  /// <param name="functionName">Namer of the exported function to call in the instane</param>
  /// <param name="args">The argument types are limited to int, long, float and double.</param>
  /// <returns>An array of values on success, null on error. You can use the LastError error property to get details on the error.</returns>
  public object [] Call (string functionName, params object [] args)
  {
    if (functionName == null)
      throw new ArgumentNullException (nameof (functionName));
    if (args == null)
      throw new ArgumentNullException (nameof (args));

    var parsOut = new WasmerValue [args.Length];
    for (int i = 0; i < args.Length; i++) {
      var tag = ImportFunction.ValidateTypeToTag (args [i].GetType ());
      parsOut [i].Tag = tag;
      switch (tag) {
        case WasmerValueType.Int32:
          parsOut [i].Storage.I32 = (int)args [i];
          break;
        case WasmerValueType.Int64:
          parsOut [i].Storage.I64 = (long)args [i];
          break;
        case WasmerValueType.Float32:
          parsOut [i].Storage.F32 = (float)args [i];
          break;
        case WasmerValueType.Float64:
          parsOut [i].Storage.F64 = (double)args [i];
          break;
      }
    }

    // TODO: need to extract array length for return and other assorted bits
    var ret = new WasmerValue [1];
    if (Call (functionName, parsOut, ret)) {
      return new object [] { ret [0].Encode () };
    }
    return null;
  }

  [DllImport (Library)]
  extern static void wasmer_instance_destroy (IntPtr handle);

  internal override Action<IntPtr> GetHandleDisposer ()
  {
    if (gchandle.IsAllocated) {
      gchandle.Free ();
    }
    return wasmer_instance_destroy;
  }

  [DllImport (Library)]
  extern static void wasmer_instance_exports (IntPtr handle, out IntPtr exportsHandle);

  /// <summary>
  /// Returns an array with all the exports - the individual values must be manually disposed.
  /// </summary>
  /// <returns></returns>
  public Exports Exports {
    get {
      wasmer_instance_exports (handle, out var exportsHandle);
      return new Exports (exportsHandle);
    }
  }

  [DllImport (Library)]
  extern static void wasmer_instance_context_data_set (IntPtr handle, IntPtr data);

  GCHandle gchandle;

  /// <summary>
  /// Sets a global data field that can be accessed by all imported functions and retrieved by the InstanceContext.Data property.
  /// </summary>
  /// <param name="value">The value to pass to all the InstanceContext members</param>
  public void SetData (object value)
  {
    gchandle = GCHandle.Alloc (value);
    wasmer_instance_context_data_set (handle, GCHandle.ToIntPtr (gchandle));
  }

  [DllImport (Library)]
  extern static InstanceContext wasmer_instance_context_get (IntPtr handle);

  /// <summary>
  /// Extracts the instance's context and returns it.
  /// </summary>
  public InstanceContext Context => wasmer_instance_context_get (handle);

}