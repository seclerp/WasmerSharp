using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

public class Exports : WasmerNativeHandle {
  internal Exports (IntPtr handle) : base (handle) { }

  [DllImport (Library)]
  extern static IntPtr wasmer_exports_get (IntPtr handle, int idx);

  [DllImport (Library)]
  extern static int wasmer_exports_len (IntPtr handle);

  [DllImport (Library)]
  extern static void wasmer_exports_destroy (IntPtr handle);

  public int Length {
    get {
      return wasmer_exports_len (handle);
    }
  }

  public Export this [int index] {
    get {
      return new Export (wasmer_exports_get (handle, index));
    }
  }
}