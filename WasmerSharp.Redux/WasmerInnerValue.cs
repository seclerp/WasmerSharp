using System.Runtime.InteropServices;

namespace WasmerSharp;

[StructLayout (LayoutKind.Explicit)]
struct WasmerInnerValue {
  [FieldOffset (0)]
  public int I32;
  [FieldOffset (0)]
  public long I64;
  [FieldOffset (0)]
  public float F32;
  [FieldOffset (0)]
  public double F64;

}