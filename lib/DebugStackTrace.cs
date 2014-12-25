
namespace ZeroMQ.lib 
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Threading;

	internal static class DebugStackTrace<TException> 
		where TException : Exception
	{

		[System.Diagnostics.DebuggerNonUserCode]
		[System.Diagnostics.DebuggerStepThrough]
		public static object Invoke(MethodInfo method, object target, params object[] args)
		{
			// source : http://csharptest.net/350/throw-innerexception-without-the-loosing-stack-trace/
            try
			{
				return method.Invoke(target, args);
			}
			catch (TException te)
			{
				if (te.InnerException == null) 
					throw;

				Exception innerException = te.InnerException;

				var savestack = (ThreadStart)Delegate.CreateDelegate(typeof(ThreadStart), innerException, "InternalPreserveStackTrace", false, false);
				if (savestack != null) savestack();

				throw innerException; // -- now we can re-throw without trashing the stack
			}
		}
	}
}

