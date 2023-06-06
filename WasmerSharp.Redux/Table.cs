using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
/// Represents a Wasmer Table.   Use the Create static method to create new instances of the table.
/// </summary>
/// <remarks>
/// <para>
/// A table is similar to a linear memory whose elements, instead of being bytes, are opaque values of a
/// particular table element type. This allows the table to contain values—like GC references,
/// raw OS handles, or native pointers—that are accessed by WebAssembly code indirectly through an integer index.
/// This feature bridges the gap between low-level, untrusted linear memory and high-level opaque
/// handles/references at the cost of a bounds-checked table indirection.
/// </para>
/// <para>
/// The table’s element type constrains the type of elements stored in the table and allows engines to
/// avoid some type checks on table use. When a WebAssembly value is stored in a table, the value’s
/// type must precisely match the element type. Depending on the operator/API used to store the value,
/// this check may be static or dynamic. Just like linear memory, updates to a table are observed
/// immediately by all instances that reference the table. Host environments may also allow storing
/// non-WebAssembly values in tables in which case, as with imports, the meaning of using the value
/// is defined by the host environment.
/// </para>
/// </remarks>
public class Table : WasmerNativeHandle {
  internal Table (IntPtr handle) : base (handle) { }

  [DllImport (Library)]
  extern static void wasmer_table_destroy (IntPtr handle);

  internal override Action<IntPtr> GetHandleDisposer ()
  {
    return wasmer_table_destroy;
  }

  [DllImport (Library)]
  extern static WasmerResult wasmer_table_new (out IntPtr handle, Limits limits);

  /// <summary>
  /// Creates a new Table for the given descriptor
  /// </summary>
  /// <param name="min">Minimum number of elements to store on the table.</param>
  /// <param name="max">Optional, maximum number of elements to store on the table.</param>
  /// <returns>An instance of Table on success, or null on error.  You can use the LastError error property to get details on the error.</returns>
  public static Table Create (uint min, uint? max = null)
  {
    Limits limits;
    limits.min = min;

    if (max.HasValue) {
      limits.max.hasSome = 1;
      limits.max.some = max.Value;
    } else {
      limits.max.hasSome = 0;
      limits.max.some = 0;
    }
    if (wasmer_table_new (out var handle, limits) == WasmerResult.Ok)
      return new Table (handle);
    else
      return null;
  }

  [DllImport (Library)]
  extern static WasmerResult wasmer_table_grow (IntPtr handle, uint delta);

  /// <summary>
  /// Attemps to grow the table by the specified number of elements.
  /// </summary>
  /// <param name="delta">Number of elements to add to the table.</param>
  /// <returns>true on success, false on failure.  You can use the LastError error property to get details on the error.</returns>
  public bool Grow (uint delta)
  {
    return wasmer_table_grow (handle, delta) == WasmerResult.Ok;
  }

  [DllImport (Library)]
  extern static uint wasmer_table_length (IntPtr handle);

  /// <summary>
  /// Returns the current length of the given Table  
  /// </summary>
  public uint Length => wasmer_table_length (handle);
}