//
// Wasmer.cs: .NET bindings to the Wasmer engine
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
using System;
using System.Runtime.InteropServices;

namespace WasmerSharp {
	internal struct WasmerByteArray {
		internal IntPtr bytes;
		internal uint bytesLen;

		public override string ToString ()
		{
			if (bytes == IntPtr.Zero)
				return null;

			unsafe {
				var len = bytesLen > Int32.MaxValue ? Int32.MaxValue : (int)bytesLen;
				return System.Text.Encoding.UTF8.GetString ((byte*)bytes, len);
			}
		}

		internal byte [] ToByteArray ()
		{
			if (bytes == IntPtr.Zero)
				return null;

			var len = bytesLen > Int32.MaxValue ? Int32.MaxValue : (int)bytesLen;
			var ret = new byte [len];
			Marshal.Copy (bytes, ret, 0, len);
			return ret;
		}

		public static WasmerByteArray FromString (string txt)
		{
			WasmerByteArray ret;

			var byteBuffer = System.Text.Encoding.UTF8.GetBytes(txt);
			ret.bytes = Marshal.AllocHGlobal (byteBuffer.Length + 1);
			Marshal.Copy (byteBuffer, 0, ret.bytes, byteBuffer.Length);
			ret.bytesLen = (uint)byteBuffer.Length;
			return ret;
		}
	}


#if false

	// Penbding bindigns: https://gist.github.com/migueldeicaza/32816d404e202840ee13ca9a7f0fe724
	public class TrampolineBufferBuilder : WasmerNativeHandle {
		internal TrampolineBufferBuilder (IntPtr handle) : base (handle) { }
	}

	public class TrampolineCallable: WasmerNativeHandle {
		internal TrampolineCallable(IntPtr handle) : base (handle) { }
	}

	public class TrampolineBuffer : WasmerNativeHandle {
		internal TrampolineBuffer (IntPtr handle) : base (handle) { }

		[DllImport (Library)]
		extern static void wasmer_trampoline_buffer_destroy (IntPtr handle);

		internal override Action<IntPtr> GetHandleDisposer ()
		{
			return wasmer_trampoline_buffer_destroy;
		}
	}
#endif
}
