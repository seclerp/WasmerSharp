using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
/// This object can wrap an int, long, float or double.   The Tag property describes the actual payload, and the I32, I64, F32 and F64 fields provide access to the underlying data.   Implicit conversion from those data types to WasmerValue exist, and explicit conversions from a WasmerValue to those types exist.
/// </summary>
[StructLayout (LayoutKind.Sequential)]
public struct WasmerValue {
  /// <summary>
  /// The underlying type for the value stored here.
  /// </summary>
  public WasmerValueType Tag;

  /// <summary>
  /// The underlying value for the value stored here.
  /// </summary>

  internal WasmerInnerValue Storage;

  /// <summary>
  /// The 32-bit integer component, when the Tag is Int32
  /// </summary>
  public int Int32 => Storage.I32;

  /// <summary>
  /// The 64-bit integer component, when the Tag is Int64
  /// </summary>
  public long Int64 => Storage.I64;
  /// <summary>
  /// The 32-bit floating point component, when the Tag is Float32
  /// </summary>
  public float Float32 => Storage.F32;
  /// <summary>
  /// The 64-bit floating point component, when the Tag is Float64
  /// </summary>
  public double Float64 => Storage.F64;

  /// <summary>
  /// Returns a boxed object that contains the underlying .NET type (int, long, float, double) based on the Tag for this value.
  /// </summary>
  /// <returns>The boxed value.</returns>
  public object Encode ()
  {
    switch (Tag) {
      case WasmerValueType.Int32:
        return Storage.I32;
      case WasmerValueType.Int64:
        return Storage.I64;
      case WasmerValueType.Float32:
        return Storage.F32;
      case WasmerValueType.Float64:
        return Storage.F64;
    }
    return null;
  }

  /// <summary>
  ///  Returns the value, suitable to be printed, the type is not shown.
  /// </summary>
  /// <returns></returns>
  public override string ToString ()
  {
    return Encode ().ToString ();
  }

  /// <summary>
  /// Returns the stored value as an int.   This will cast if the value is not a native int.
  /// </summary>
  /// <param name="val">The incoming WasmerValue.</param>
  public static explicit operator int (WasmerValue val)
  {
    switch (val.Tag) {
      case WasmerValueType.Int32:
        return val.Storage.I32;

      case WasmerValueType.Int64:
        return (int)val.Storage.I64;

      case WasmerValueType.Float32:
        return (int)val.Storage.F32;

      case WasmerValueType.Float64:
        return (int)val.Storage.F64;
    }
    throw new Exception ("Unknown WasmerValueType");
  }

  /// <summary>
  /// Returns the stored value as a long.   This will cast if the value is not a native long.
  /// </summary>
  /// <param name="val">The incoming WasmerValue.</param>
  public static explicit operator long (WasmerValue val)
  {
    switch (val.Tag) {
      case WasmerValueType.Int32:
        return val.Storage.I32;

      case WasmerValueType.Int64:
        return val.Storage.I64;

      case WasmerValueType.Float32:
        return (long)val.Storage.F32;

      case WasmerValueType.Float64:
        return (long)val.Storage.F64;
    }
    throw new Exception ("Unknown WasmerValueType");
  }

  /// <summary>
  /// Returns the stored value as a float.   This will cast if the value is not a native float.
  /// </summary>
  /// <param name="val">The incoming WasmerValue.</param>
  public static explicit operator float (WasmerValue val)
  {
    switch (val.Tag) {
      case WasmerValueType.Int32:
        return val.Storage.I32;

      case WasmerValueType.Int64:
        return (float)val.Storage.I64;

      case WasmerValueType.Float32:
        return val.Storage.F32;

      case WasmerValueType.Float64:
        return (float)val.Storage.F64;
    }
    throw new Exception ("Unknown WasmerValueType");
  }

  /// <summary>
  /// Returns the stored value as a double.   This will cast if the value is not a native double.
  /// </summary>
  /// <param name="val">The incoming WasmerValue.</param>

  public static explicit operator double (WasmerValue val)
  {
    switch (val.Tag) {
      case WasmerValueType.Int32:
        return val.Storage.I32;

      case WasmerValueType.Int64:
        return (double)val.Storage.I64;

      case WasmerValueType.Float32:
        return val.Storage.F32;

      case WasmerValueType.Float64:
        return val.Storage.F64;
    }
    throw new Exception ("Unknown WasmerValueType");
  }

  /// <summary>
  /// Creates a WasmerValue from an integer
  /// </summary>
  /// <param name="val">Integer value to wrap</param>
  public static implicit operator WasmerValue (int val)
  {
    return new WasmerValue () { Tag = WasmerValueType.Int32, Storage = new WasmerInnerValue () { I32 = val } };
  }

  /// <summary>
  /// Creates a WasmerValue from an long
  /// </summary>
  /// <param name="val">Long value to wrap</param>
  public static implicit operator WasmerValue (long val)
  {
    return new WasmerValue () { Tag = WasmerValueType.Int64, Storage = new WasmerInnerValue () { I64 = val } };
  }

  /// <summary>
  /// Creates a WasmerValue from an float
  /// </summary>
  /// <param name="val">Float value to wrap</param>
  public static implicit operator WasmerValue (float val)
  {
    return new WasmerValue () { Tag = WasmerValueType.Float32, Storage = new WasmerInnerValue () { F32 = val } };
  }

  /// <summary>
  /// Creates a WasmerValue from an double
  /// </summary>
  /// <param name="val">Double value to wrap</param>
  public static implicit operator WasmerValue (double val)
  {
    return new WasmerValue () { Tag = WasmerValueType.Float64, Storage = new WasmerInnerValue () { F64 = val } };
  }

}