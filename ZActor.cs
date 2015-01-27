namespace ZeroMQ
{
	using System;
	// using System.Threading;
	using System.Collections.Generic;

	public delegate void ZAction(object[] args, ZSocket backend, ZActor actor);

	public class ZActor : ZThread
	{
		public ZSocket Frontend { get; protected set; }

		public ZSocket Backend { get; protected set; }

		public string Endpoint { get; protected set; }

		public ZAction Action { get; protected set; }

		public object[] Arguments { get; protected set; }

		protected ZActor ()
		{ }

		public static ZActor Create(ZContext context, ZAction action, params object[] args)
		{
			var rnd = new Random();
			var endpoint = string.Format("inproc://{0}-{1}", rnd.Next(), rnd.Next());

			return Create(context, endpoint, action, args);
		}

		public static ZActor Create(ZContext context, string endpoint, ZAction action, params object[] args)
		{
			var actor = new ZActor();

			actor.Frontend = ZSocket.Create(context, ZSocketType.PAIR);
			actor.Backend = ZSocket.Create(context, ZSocketType.PAIR);

			actor.Endpoint = endpoint;
			actor.Action = action;
			actor.Arguments = args;

			return actor;
		}

		protected override void Run()
		{
			Backend.Bind(string.Format("inproc://{0}", Endpoint));
			Frontend.Connect(string.Format("inproc://{0}", Endpoint));

			Action(Arguments, Backend, this);
		}

		public virtual new ZActor Start()
		{
			return (ZActor)base.Start();
		}
	}
}