using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
/// Modules can either be serialized to byte arrays, or created from a serialized state (byte arrays).  This class provides this bridge.
/// </summary>
public class SerializedModule : WasmerNativeHandle {
  internal SerializedModule (IntPtr handle) : base (handle) { }

  [DllImport (Library)]
  extern static void wasmer_serialized_module_destroy (IntPtr handle);

  [DllImport (Library)]
  extern static WasmerResult wasmer_serialized_module_from_bytes (out IntPtr handle, IntPtr bytes, uint len);

  /// <summary>
  /// Creates a new SerializedModule from the provided buffer.
  /// </summary>
  /// <param name="bytes">Pointer to a region in memory containing the serialized module.</param>
  /// <param name="len">The number of bytes toe process from the buffer</param>
  /// <returns>Returns null on error, or an instance of SerializeModule on success.  You can use the LastError error property to get details on the error.</returns>
  public static SerializedModule FromBytes (IntPtr bytes, uint len)
  {
    if (wasmer_serialized_module_from_bytes (out var handle, bytes, len) == WasmerResult.Ok)
      return new SerializedModule (handle);
    else
      return null;
  }

  /// <summary>
  /// Creates a new SerializedModule from the provided byte array
  /// </summary>
  /// <param name="buffer">Array of bytes containing the serialized module.</param>
  /// <returns>Returns null on error, or an instance of SerializeModule on success.   You can use the LastError error property to get details on the error.</returns>
  public static SerializedModule FromBytes (byte [] buffer)
  {
    unsafe {
      fixed (byte* p = &buffer [0]) {
        return FromBytes ((IntPtr)p, (uint)buffer.Length);
      }
    }
  }

  [DllImport (Library)]
  extern static WasmerByteArray wasmer_serialized_module_bytes (IntPtr handle);

  /// <summary>
  /// Returns the serialized module as a byte array.
  /// </summary>
  /// <returns>The byte array for this serialized module</returns>
  public byte [] GetModuleBytes ()
  {
    return wasmer_serialized_module_bytes (handle).ToByteArray ();
  }

  internal override Action<IntPtr> GetHandleDisposer ()
  {
    return wasmer_serialized_module_destroy;
  }

  [DllImport (Library)]
  extern static WasmerResult wasmer_module_deserialize (out IntPtr module, IntPtr handle);

  /// <summary>
  /// Deserialize the given serialized module.
  /// </summary>
  /// <returns>Returns an instance of a Module, or null on error.  You can use the LastError error property to get details on the error. </returns>
  public Module Deserialize ()
  {
    if (wasmer_module_deserialize (out var moduleHandle, handle) == WasmerResult.Ok)
      return new Module (moduleHandle);
    return null;
  }
}