using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WasmerSharp;

/// <summary>
/// This wraps a native handle and takes care of disposing the handles they wrap.
/// Due to the design of the Wasmer API that can
/// </summary>
/// <remarks>
/// produce a lot of values that need to be destroyed, and in an effort to balance
/// the complexity that it would involve, this queues releases of data on either
/// construction or on the main thread dispose method.
/// </remarks>
public class WasmerNativeHandle : IDisposable {
  static Queue<Tuple<Action<IntPtr>, IntPtr>> pendingReleases = new Queue<Tuple<Action<IntPtr>, IntPtr>> ();
  internal IntPtr handle;

  internal const string Library = "wasmer_runtime_c_api";

  /// <summary>
  ///  Releases any pending objects that were queued for destruction
  /// </summary>
  internal static void Flush ()
  {
    while (pendingReleases.Count > 0) {
      var v = pendingReleases.Dequeue ();
      v.Item1 (v.Item2);
    }
  }

  internal WasmerNativeHandle (IntPtr handle)
  {
    this.handle = handle;
    Flush ();
  }

  internal WasmerNativeHandle ()
  {
    Flush ();
  }

  /// <summary>
  ///  Disposes the object, releasing the unmanaged resources associated with it.
  /// </summary>
  public void Dispose ()
  {
    Dispose (true);
    GC.SuppressFinalize (this);
  }

  /// <summary>
  /// This method when called with disposing, should dispose right away,
  /// otherwise it should return an Action of IntPtr that can dispose the handle.
  /// </summary>
  /// <returns>The method to invoke on disposing, or null if there is no need to dispose</returns>
  internal virtual Action<IntPtr> GetHandleDisposer ()
  {
    return null;
  }

  internal virtual void Dispose (bool disposing)
  {
    var handleDisposer = GetHandleDisposer ();
    if (disposing) {
      if (handleDisposer != null)
        handleDisposer (handle);
      Flush ();
    } else if (handleDisposer != null) {
      lock (pendingReleases) {
        pendingReleases.Enqueue (new Tuple<Action<IntPtr>, IntPtr> (handleDisposer, handle));
      }
    }
    handle = IntPtr.Zero;
  }

  [DllImport (Library)]
  extern static int wasmer_last_error_length ();
  [DllImport (Library)]
  extern static int wasmer_last_error_message (IntPtr buffer, int len);

  /// <summary>
  /// Returns the last error message that was raised by the Wasmer Runtime
  /// </summary>
  public string LastError {
    get {
      var len = wasmer_last_error_length ();
      unsafe {
        var buf = Marshal.AllocHGlobal (len);
        wasmer_last_error_message (buf, len);
        var str = System.Text.Encoding.UTF8.GetString ((byte*)buf, len);

        Marshal.FreeHGlobal (buf);
        return str;
      }
    }
  }

}