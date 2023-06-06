using System.Runtime.InteropServices;

namespace WasmerSharp;

[StructLayout (LayoutKind.Sequential)]
internal struct GlobalDescriptor {
  internal byte Mutable;
  internal WasmerValueType Type;
}