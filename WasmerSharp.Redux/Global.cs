using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
/// Represents a Global variable instance, importable/exportable across multiple modules.
/// </summary>
public class Global : WasmerNativeHandle {
  internal Global (IntPtr handle) : base (handle) { }

  // To avoid the warnings about the "fill" members here
#pragma warning disable 649
  struct IntValue {
    public WasmerValueType t;
    public int payload;
    public int fill;
  }
  struct LongValue {
    public WasmerValueType t;
    public long payload;
  }
  struct FloatValue {
    public WasmerValueType t;
    public float payload;
    public int fill;
  }
  struct DoubleValue {
    public WasmerValueType t;
    public double payload;
  }
#pragma warning restore 649

  [DllImport (Library)]
  extern static IntPtr wasmer_global_new (IntValue value, [MarshalAs (UnmanagedType.U1)] bool mutable);
  [DllImport (Library)]
  extern static IntPtr wasmer_global_new (LongValue value, [MarshalAs (UnmanagedType.U1)] bool mutable);
  [DllImport (Library)]
  extern static IntPtr wasmer_global_new (FloatValue value, [MarshalAs (UnmanagedType.U1)] bool mutable);
  [DllImport (Library)]
  extern static IntPtr wasmer_global_new (DoubleValue value, [MarshalAs (UnmanagedType.U1)] bool mutable);

  /// <summary>
  /// Creates a new integer global with the specified WasmerValue.
  /// </summary>
  /// <param name="val">The value to place on the global</param>
  /// <param name="mutable">Determines whether the global is mutable</param>
  public Global (int val, bool mutable)
  {
    var x = new IntValue () { t = WasmerValueType.Int32, payload = val };
    handle = wasmer_global_new (x, mutable);
  }

  /// <summary>
  /// Creates a new long global with the specified WasmerValue.
  /// </summary>
  /// <param name="val">The value to place on the global</param>
  /// <param name="mutable">Determines whether the global is mutable</param>
  public Global (long val, bool mutable)
  {
    var x = new LongValue () { t = WasmerValueType.Int64, payload = val };
    handle = wasmer_global_new (x, mutable);
  }

  /// <summary>
  /// Creates a new float global with the specified WasmerValue.
  /// </summary>
  /// <param name="val">The value to place on the global</param>
  /// <param name="mutable">Determines whether the global is mutable</param>
  public Global (float val, bool mutable)
  {
    var x = new FloatValue () { t = WasmerValueType.Float32, payload = val };
    handle = wasmer_global_new (x, mutable);
  }

  /// <summary>
  /// Creates a new double global with the specified WasmerValue.
  /// </summary>
  /// <param name="val">The value to place on the global</param>
  /// <param name="mutable">Determines whether the global is mutable</param>
  public Global (double val, bool mutable)
  {
    var x = new DoubleValue () { t = WasmerValueType.Float64, payload = val };
    handle = wasmer_global_new (x, mutable);
  }

  [DllImport (Library)]
  extern static void wasmer_global_destroy (IntPtr handle);

  internal override Action<IntPtr> GetHandleDisposer ()
  {
    return wasmer_global_destroy;
  }

  [DllImport (Library)]
  extern static WorkaroundWasmerValue wasmer_global_get (IntPtr handle);

  [StructLayout (LayoutKind.Sequential)]
  struct WorkaroundWasmerValue {
    int x;
    long y;
  }

  /// <summary>
  /// Returns the value stored in this global
  /// </summary>
  public WasmerValue Value {
    get {
      // Need these gymnastics in CoreCLR because it chokes with
      // unions.
      var result = wasmer_global_get (handle);
      unsafe {
        return *(WasmerValue*)(&result);
      }
    }
  }

  [DllImport (Library)]
  extern static GlobalDescriptor wasmer_global_get_descriptor (IntPtr global);

  /// <summary>
  /// Determines whether this Global is mutable or not.
  /// </summary>
  public bool IsMutable {
    get {
      return wasmer_global_get_descriptor (handle).Mutable != 0;
    }
  }

  /// <summary>
  /// Returns the ValueType (type) of the global.
  /// </summary>
  public WasmerValueType ValueType {
    get {
      return wasmer_global_get_descriptor (handle).Type;
    }
  }

  [DllImport (Library)]
  extern static void wasmer_global_set (IntPtr global, WorkaroundWasmerValue value);

  /// <summary>
  /// Sets the value of the global to the provided value, which can be a WasmerValue, or an int, long, float or double
  /// </summary>
  /// <param name="value">The new value to set</param>
  public void Set (WasmerValue value)
  {
    unsafe {
      WorkaroundWasmerValue x;

      x = *(WorkaroundWasmerValue*)(&value);
      wasmer_global_set (handle, x);
    }
  }
}