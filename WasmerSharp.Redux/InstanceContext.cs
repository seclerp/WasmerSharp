using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
/// An instance of this type is provided as the first parameter of imported functions and can be used
/// to get some contextual information from the callback to operate on: the global Data set for the
/// instance as well as the associated memory.
/// </summary>
public struct InstanceContext {
  /// <summary>
  ///   Handle to the underlying wasmer_instance_context_t *
  /// </summary>
  public IntPtr Handle;

  [DllImport (WasmerNativeHandle.Library)]
  extern static IntPtr wasmer_instance_context_data_get (IntPtr handle);

  /// <summary>
  /// Retrieves the global Data value that was set for this Instance.
  /// </summary>
  public object Data => GCHandle.FromIntPtr (wasmer_instance_context_data_get (Handle));

  [DllImport (WasmerNativeHandle.Library)]
  extern static IntPtr wasmer_instance_context_memory (IntPtr handle, uint memoryId);

  /// <summary>
  /// The memory blob associated with the instance.   Currently this only supports idx=0
  /// </summary>
  /// <param name="idx">The index of the memory to retrieve, currently only supports one memory blob.</param>
  public Memory GetMemory (uint idx)
  {
    var b = wasmer_instance_context_memory (Handle, idx);
    if (b == IntPtr.Zero)
      return null;
    return new Memory (b, owns: false);
  }
}