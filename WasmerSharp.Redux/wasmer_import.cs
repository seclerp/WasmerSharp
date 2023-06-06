using System;
using System.Runtime.InteropServices;

namespace WasmerSharp;

[StructLayout (LayoutKind.Sequential)]
internal struct wasmer_import {
  internal WasmerByteArray module_name;
  internal WasmerByteArray import_name;
  internal ImportExportKind tag;
  internal IntPtr value;
}