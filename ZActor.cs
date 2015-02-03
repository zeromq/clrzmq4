namespace ZeroMQ
{
	using System;
	using System.Threading;
	using System.Collections.Generic;

	public delegate void ZAction(ZContext context, ZSocket backend, CancellationTokenSource cancellor, object[] args);

	public class ZActor : ZThread
	{
		public ZContext Context { get; protected set; }

		public ZSocket Frontend { get; protected set; }

		public string Endpoint { get; protected set; }

		protected ZSocket Backend { get; set; }

		protected ZAction Action { get; set; }

		protected object[] Arguments { get; set; }

		static readonly Random rnd = new Random();

		public ZActor (ZContext context, ZAction action, params object[] args)
			: this (context, default(string), action, args)
		{
			this.Endpoint = string.Format("inproc://{0}-{1}", rnd.Next(), rnd.Next());
		}

		public ZActor(ZContext context, string endpoint, ZAction action, params object[] args)
			: base ()
		{
			this.Context = context;

			this.Endpoint = endpoint;
			this.Action = action;
			this.Arguments = args;
		}

		protected override void Run()
		{
			using (Backend = ZSocket.Create(Context, ZSocketType.PAIR))
			{
				Backend.Bind(Endpoint);

				Action(Context, Backend, Cancellor, Arguments);
			}
		}

		public override void Start()
		{
			base.Start();

			if (Frontend == null)
			{
				Frontend = ZSocket.Create(Context, ZSocketType.PAIR);
				Frontend.Connect(Endpoint);
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (Frontend != null)
				{
					Frontend.Dispose();
					Frontend = null;
				}
				if (Backend != null)
				{
					Backend.Dispose();
					Backend = null;
				}
			}
		}
	}
}