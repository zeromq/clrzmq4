namespace ZeroMQ
{
	public delegate void ZAction(ZContext context, ZSocket backend, System.Threading.CancellationTokenSource cancellor, object[] args);

	public class ZActor : ZThread
	{
		public ZContext Context { get; protected set; }

		public string Endpoint { get; protected set; }

		public ZAction Action { get; protected set; }

		public object[] Arguments { get; protected set; }

		public ZSocket Backend { get; protected set; }

		public ZSocket Frontend { get; protected set; }

		public ZActor(ZAction action, params object[] args)
			: this(ZContext.Current, action, args)
		{ }

		public ZActor (ZContext context, ZAction action, params object[] args)
			: this (context, default(string), action, args)
		{
			var rnd0 = new byte[8];
			using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider()) rng.GetNonZeroBytes(rnd0);
			this.Endpoint = string.Format("inproc://{0}", ZContext.Encoding.GetString(rnd0));
		}

		public ZActor(string endpoint, ZAction action, params object[] args)
			: this (ZContext.Current, endpoint, action, args)
		{ }

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