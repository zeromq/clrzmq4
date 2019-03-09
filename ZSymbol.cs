// using System.Runtime.Remoting.Messaging;

namespace ZeroMQ
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using lib;

    /// <summary>
    /// TODO merge this with its sole subclass, ZError
    /// </summary>
    public class ZSymbol
	{
		internal protected ZSymbol(int errno)
		{
			this._num = errno;
		}

		private int _num;
		public int Number { get { return _num; } }

        public string Name
        {
            get
            {
                string result;
                return _zsymbolToName.TryGetValue(this, out result) ? result : "<unknown>";
            }
        }

        public string Text { get { return Marshal.PtrToStringAnsi(zmq.strerror(_num)); }  }

		private static void PickupConstantSymbols<T>(ref IDictionary<ZSymbol, string> symbols)
            where T : ZSymbol
		{
			Type type = typeof(T);

			FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

			Type codeType = type.GetNestedType("Code", BindingFlags.NonPublic);

			// Pickup constant symbols
			foreach (FieldInfo symbolField in fields.Where(f => typeof(ZSymbol).IsAssignableFrom(f.FieldType)))
			{
				FieldInfo symbolCodeField = codeType.GetField(symbolField.Name);
				if (symbolCodeField != null)
				{
					int symbolNumber = (int)symbolCodeField.GetValue(null);

				    var symbol = Activator.CreateInstance(
				        type,
				        BindingFlags.NonPublic | BindingFlags.Instance, 
				        null,
				        new object[] {symbolNumber},
				        null);
					symbolField.SetValue(null, symbol);
					symbols.Add((ZSymbol)symbol, symbolCodeField.Name);
				}
			}
		}

		public static readonly ZSymbol None = default(ZSymbol);

		static ZSymbol()
		{
			// System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(zmq).TypeHandle);

			IDictionary<ZSymbol, string> symbols = new Dictionary<ZSymbol, string>();

			PickupConstantSymbols<ZError>(ref symbols);

			_zsymbolToName = symbols;
		}

        [Obsolete]
		public bool IsEmpty()
		{
            // TODO: what is the intended semantics of this method? The following expression is always false, since default(ZSymbol) == null.
			return this == default(ZSymbol);
		}

		static IDictionary<ZSymbol, string> _zsymbolToName;

		public static IEnumerable<ZSymbol> Find(string symbol)
		{
			return _zsymbolToName
				.Where(s => s.Value != null && (s.Value == symbol)).Select(x => x.Key);
		}

		public static IEnumerable<ZSymbol> Find(string ns, int num)
		{
			return _zsymbolToName
				.Where(s => s.Value != null && (s.Value.StartsWith(ns) && s.Key._num == num)).Select(x => x.Key);
		}

		public override bool Equals(object obj)
		{
			return ZSymbol.Equals(this, obj);
		}

	    public new static bool Equals(object a, object b)
	    {
	        if (object.ReferenceEquals(a, b))
	        {
	            return true;
	        }

	        var symbolA = a as ZSymbol;
	        var symbolB = b as ZSymbol;

	        return symbolA != null && symbolB != null && symbolA._num == symbolB._num;
	    }

		public override int GetHashCode()
		{
			return Number.GetHashCode();
		}

		public override string ToString()
		{
			return Name + "(" + Number + "): " + Text;
		}

		public static implicit operator int(ZSymbol errnum)
		{
			return errnum.Number;
		}

	}
}