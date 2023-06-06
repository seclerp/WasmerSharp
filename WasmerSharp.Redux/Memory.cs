using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
/// Represents the WebAssembly memory.   Memory is allocated in pages, which are 64k bytes in size.
/// </summary>
public class Memory : WasmerNativeHandle {
  bool owns;
  internal Memory (IntPtr handle, bool owns = true) : base (handle)
  {
    this.owns = owns;
  }

  [DllImport (Library)]
  extern static WasmerResult wasmer_memory_new (out IntPtr handle, Limits limits);

  /// <summary>
  ///  Creates a memory block with the specified minimum and maxiumum limits
  /// </summary>
  /// <param name="minPages">Minimum number of allowed pages</param>
  /// <param name="maxPages">Optional, Maximum number of allowed pages</param>
  /// <returns>The object on success, or null on failure. You can use the LastError error property to get details on the error.</returns>
  public static Memory Create (uint minPages, uint? maxPages = null)
  {
    Limits limits;
    limits.min = minPages;

    if (maxPages.HasValue) {
      limits.max.hasSome = 1;
      limits.max.some = maxPages.Value;
    } else {
      limits.max.hasSome = 0;
      limits.max.some = 0;
    }
    if (wasmer_memory_new (out var handle, limits) == WasmerResult.Ok) {
      return new Memory (handle);
    }
    return null;
  }

  /// <summary>
  ///  Constructor for memory, throws if there is an error.
  /// </summary>
  /// <param name="minPages">Minimum number of allowed pages</param>
  /// <param name="maxPages">Optional, Maximum number of allowed pages</param>

  public Memory (uint minPages, uint? maxPages = null)
  {
    Limits limits;
    limits.min = minPages;

    if (maxPages.HasValue) {
      limits.max.hasSome = 1;
      limits.max.some = maxPages.Value;
    } else {
      limits.max.hasSome = 0;
      limits.max.some = 0;
    }

    if (wasmer_memory_new (out var xhandle, limits) == WasmerResult.Ok)
      handle = xhandle;
    else
      throw new Exception ("Error creating the requested memory");
  }

  [DllImport (Library)]
  extern static void wasmer_memory_destroy (IntPtr handle);

  internal override Action<IntPtr> GetHandleDisposer ()
  {
    if (owns)
      return wasmer_memory_destroy;
    return null;
  }

  [DllImport (Library)]
  extern static WasmerResult wasmer_memory_grow (IntPtr handle, uint data);

  /// <summary>
  /// Grows the memory by the specified amount of pages.
  /// </summary>
  /// <param name="deltaPages">The number of additional pages to grow</param>
  /// <returns>true on success, false on error.   You can use the LastError property to get more details on the error.</returns>
  public bool Grow (uint deltaPages)
  {
    return wasmer_memory_grow (handle, deltaPages) != 0;
  }

  [DllImport (Library)]
  extern static uint wasmer_memory_length (IntPtr handle);

  /// <summary>
  /// Returns the current length in pages of the given memory 
  /// </summary>
  public uint PageLength => wasmer_memory_length (handle);

  [DllImport (Library)]
  extern static uint wasmer_memory_data_length (IntPtr handle);

  /// <summary>
  /// Returns the current length in bytes of the given memory 
  /// </summary>
  public uint DataLength => wasmer_memory_data_length (handle);

  [DllImport (Library)]
  extern static IntPtr wasmer_memory_data (IntPtr handle);

  /// <summary>
  /// Returns a pointer to the memory backing this Memory instance.
  /// </summary>
  public IntPtr Data => wasmer_memory_data (handle);

  /// <summary>
  ///  Returns a human-readable description of the Memory resource
  /// </summary>
  public override string ToString ()
  {
    return $"{DataLength} bytes at address {(ulong)Data:x}";
  }
}