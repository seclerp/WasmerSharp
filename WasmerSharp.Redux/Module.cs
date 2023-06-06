using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
/// Represents a WebAssembly module, created from a byte array containing the WebAssembly code.
/// </summary>
/// <remarks>
///    Use the Create method to create new instances of a module.
/// </remarks>
public class Module : WasmerNativeHandle {
  internal Module (IntPtr handle) : base (handle) { }

  [DllImport (Library)]
  extern static WasmerResult wasmer_compile (out IntPtr handle, IntPtr body, uint len);

  /// <summary>
  /// Creates a new Module from the given WASM bytes pointed to by the specified address
  /// </summary>
  /// <param name="wasmBody">A pointer to a block of memory containing the WASM code to load into the module</param>
  /// <param name="bodyLength">The size of the wasmBody pointer</param>
  /// <returns>The WasmerModule instance, or null on error.   You can use the LastError error property to get details on the error.</returns>
  public static Module Create (IntPtr wasmBody, uint bodyLength)
  {
    if (wasmer_compile (out var handle, wasmBody, bodyLength) == WasmerResult.Ok) {
      return new Module (handle);
    } else
      return null;
  }

  /// <summary>
  /// Creates a new Module from the given WASM bytes
  /// </summary>
  /// <param name="wasmBody">An array containing the WASM code to load into the module</param>
  /// <returns>The WasmerModule instance, or null on error.  You can use the LastError error property to get details on the error.</returns>
  public static Module Create (byte [] wasmBody)
  {
    if (wasmBody == null)
      throw new ArgumentException (nameof (wasmBody));
    unsafe {
      fixed (byte* p = &wasmBody [0]) {
        return Create ((IntPtr)p, (uint)wasmBody.Length);
      }
    }
  }

  [DllImport (Library)]
  extern static void wasmer_export_descriptors (IntPtr handle, out IntPtr exportDescs);

  [DllImport (Library)]
  extern static void wasmer_export_descriptors_destroy (IntPtr handle);

  [DllImport (Library)]
  extern static int wasmer_export_descriptors_len (IntPtr handle);

  [DllImport (Library)]
  extern static IntPtr wasmer_export_descriptors_get (IntPtr descsHandle, int idx);

  [DllImport (Library)]
  extern static ImportExportKind wasmer_export_descriptor_kind (IntPtr handle);
  [DllImport (Library)]
  extern static WasmerByteArray wasmer_export_descriptor_name (IntPtr handle);

  /// <summary>
  /// Gets export descriptors for the given module
  /// </summary>
  public ExportDescriptor [] ExportDescriptors {
    get {
      // Not worth surfacing all the Disposable junk, so we extract all the data in one go
      wasmer_export_descriptors (handle, out var exportsHandle);
      int len = wasmer_export_descriptors_len (handle);
      var result = new ExportDescriptor [len];
      for (int i = 0; i < len; i++) {
        var dhandle = wasmer_export_descriptors_get (exportsHandle, i);

        result [i] = new ExportDescriptor () {
          Kind = wasmer_export_descriptor_kind (dhandle),
          Name = wasmer_export_descriptor_name (dhandle).ToString ()
        };
      }
      wasmer_export_descriptors_destroy (exportsHandle);
      return result;
    }
  }

  [DllImport (Library)]
  unsafe extern static WasmerResult wasmer_module_instantiate (IntPtr handle, out IntPtr instance, wasmer_import* imports, int imports_len);

  /// <summary>
  /// Creates a new Instance from the given wasm bytes and imports. 
  /// </summary>
  /// <param name="imports">The list of imports to pass, usually Function, Global and Memory</param>
  /// <returns>A Wasmer.Instance on success, or null on error.   You can use the LastError error property to get details on the error.</returns>
  public Instance Instantiate (params Import [] imports)
  {
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
        if (wasmer_module_instantiate (handle, out var result, p, llimports.Length) == WasmerResult.Ok)
          return new Instance (result);
        else
          return null;
      }
    }
  }

  [DllImport (Library)]
  extern static void wasmer_module_destroy (IntPtr handle);

  internal override Action<IntPtr> GetHandleDisposer ()
  {
    return wasmer_module_destroy;
  }

  [DllImport (Library)]
  extern static void wasmer_import_descriptors (IntPtr moduleHandle, out IntPtr importDescriptors);

  [DllImport (Library)]
  extern static void wasmer_import_descriptors_destroy (IntPtr handle);

  [DllImport (Library)]
  extern static IntPtr wasmer_import_descriptors_get (IntPtr descriptorsHandle, int idx);

  [DllImport (Library)]
  extern static int wasmer_import_descriptors_len (IntPtr handle);

  [DllImport (Library)]
  extern static ImportExportKind wasmer_import_descriptor_kind (IntPtr handle);
  [DllImport (Library)]
  extern static WasmerByteArray wasmer_import_descriptor_module_name (IntPtr handle);
  [DllImport (Library)]
  extern static WasmerByteArray wasmer_import_descriptor_name (IntPtr handle);

  /// <summary>
  /// Returns the Import Descriptors for this module
  /// </summary>
  public ImportDescriptor [] ImportDescriptors {
    get {
      wasmer_import_descriptors (handle, out var importsHandle);
      var len = wasmer_import_descriptors_len (importsHandle);
      var res = new ImportDescriptor [len];
      for (int i = 0; i < len; i++) {
        var id = wasmer_import_descriptors_get (importsHandle, i);

        // Need to prepopulate, as it looks like destroying the container
        // also destroy the underlyiung data structures used by these.
        res [i] = new ImportDescriptor () {
          Kind = wasmer_import_descriptor_kind (id),
          ModuleName = wasmer_import_descriptor_module_name (id).ToString (),
          Name = wasmer_import_descriptor_name (id).ToString ()
        };
      }
      wasmer_import_descriptors_destroy (importsHandle);
      return res;
    }
  }

  [DllImport (Library)]
  extern static WasmerResult wasmer_module_serialize (out IntPtr serialized, IntPtr handle);

  /// <summary>
  /// Serializes the module, the result can be turned into a byte array and saved.
  /// </summary>
  /// <returns>Null on error, or an instance of SerializedModule on success.  You can use the LastError error property to get details on the error.</returns>
  public SerializedModule Serialize ()
  {
    if (wasmer_module_serialize (out var serialized, handle) == WasmerResult.Ok) {
      return new SerializedModule (serialized);
    } else
      return null;
  }

  [DllImport (Library)]
  extern static byte wasmer_validate (IntPtr bytes, uint len);

  /// <summary>
  /// Validates a block of bytes for being a valid web assembly package.
  /// </summary>
  /// <param name="bytes">Pointer to the bytes that contain the webassembly payload</param>
  /// <param name="len">Length of the buffer.</param>
  /// <returns>True if this contains a valid webassembly package, false otherwise.</returns>
  public bool Validate (IntPtr bytes, uint len)
  {
    return wasmer_validate (bytes, len) != 0;
  }

  /// <summary>
  /// Validates a byte array for being a valid web assembly package.
  /// </summary>
  /// <param name="buffer">Array containing the webassembly package to validate</param>
  /// <returns>True if this contains a valid webassembly package, false otherwise.</returns>
  public bool Validate (byte [] buffer)
  {
    unsafe {
      fixed (byte* p = &buffer [0]) {
        return Validate ((IntPtr)p, (uint)buffer.Length);
      }
    }
  }

}