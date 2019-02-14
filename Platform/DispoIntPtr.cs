using System.Threading;

namespace ZeroMQ.lib
{
	using System;
	using System.Text;
	using System.Threading;
	using System.Runtime.InteropServices;

    internal sealed partial class DispoIntPtr : IDisposable
	{
		private delegate DispoIntPtr AllocStringNativeDelegate(string str, out int byteCount);

		private static readonly AllocStringNativeDelegate AllocStringNative = Ansi.AllocStringNative;

		/* static DispoIntPtr() {
			// Platform.SetupPlatformImplementation(typeof(DispoIntPtr));
		} */

		public static DispoIntPtr Alloc(int size)
		{
			var dispPtr = new DispoIntPtr();
			dispPtr._ptr = Marshal.AllocHGlobal(size);
			dispPtr.isAllocated = true;
			return dispPtr;
		}

		public static DispoIntPtr AllocString(string str)
		{
			int byteCount;
			return AllocString(str, out byteCount);
		}

		public static DispoIntPtr AllocString(string str, out int byteCount)
		{
			return AllocStringNative(str, out byteCount);
		}

		public static implicit operator IntPtr(DispoIntPtr dispoIntPtr)
		{
			return dispoIntPtr == null ? IntPtr.Zero : dispoIntPtr._ptr;
		}

		unsafe public static explicit operator void*(DispoIntPtr dispoIntPtr)
		{
			return dispoIntPtr == null ? (void*)null : (void*)dispoIntPtr._ptr;
		}

		unsafe public static explicit operator byte*(DispoIntPtr dispoIntPtr)
		{
			return dispoIntPtr == null ? (byte*)null : (byte*)dispoIntPtr._ptr;
		}

		unsafe public static explicit operator sbyte*(DispoIntPtr dispoIntPtr)
		{
			return dispoIntPtr == null ? (sbyte*)null : (sbyte*)dispoIntPtr._ptr;
		}

		unsafe public static explicit operator short*(DispoIntPtr dispoIntPtr)
		{
			return dispoIntPtr == null ? (short*)null : (short*)dispoIntPtr._ptr;
		}

		unsafe public static explicit operator ushort*(DispoIntPtr dispoIntPtr)
		{
			return dispoIntPtr == null ? (ushort*)null : (ushort*)dispoIntPtr._ptr;
		}

		unsafe public static explicit operator char*(DispoIntPtr dispoIntPtr)
		{
			return dispoIntPtr == null ? (char*)null : (char*)dispoIntPtr._ptr;
		}

		unsafe public static explicit operator int*(DispoIntPtr dispoIntPtr)
		{
			return dispoIntPtr == null ? (int*)null : (int*)dispoIntPtr._ptr;
		}

		unsafe public static explicit operator uint*(DispoIntPtr dispoIntPtr)
		{
			return dispoIntPtr == null ? (uint*)null : (uint*)dispoIntPtr._ptr;
		}

		unsafe public static explicit operator long*(DispoIntPtr dispoIntPtr)
		{
			return dispoIntPtr == null ? (long*)null : (long*)dispoIntPtr._ptr;
		}

		unsafe public static explicit operator ulong*(DispoIntPtr dispoIntPtr)
		{
			return dispoIntPtr == null ? (ulong*)null : (ulong*)dispoIntPtr._ptr;
		}

		private bool isAllocated;

		private IntPtr _ptr;

		public IntPtr Ptr
		{
			get { return _ptr; }
		}

		internal DispoIntPtr() { }

		~DispoIntPtr()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			// TODO: instance ThreadStatic && do ( o == null ? return : ( lock(o, ms), check threadId, .. ) ) 
			IntPtr handle = _ptr;
			if (handle != IntPtr.Zero)
			{
				if (isAllocated)
				{
					Marshal.FreeHGlobal(handle);
					isAllocated = false;
				}
				_ptr = IntPtr.Zero;
			}
		}

		/* public void ReAlloc(long size) {
			_ptr = Marshal.ReAllocHGlobal(_ptr, new IntPtr(size));
		} */
	}
}