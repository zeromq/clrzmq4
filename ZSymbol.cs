namespace ZeroMQ
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using lib;

	public abstract class ZSymbol
	{
		public ZSymbol(int errno)
			: this(errno, null, null) { }

		public ZSymbol(int errno, string errname, string errtext)
		{
			this._num = errno;
			this._name = errname;
			this._txt = errtext;
		}

		private int _num;
		public int Number { get { return _num; } protected set { _num = value; } }

		private string _name;
		public string Name { get { return _name; } protected set { _name = value; } }

		private string _txt;
		public string Text { get { return _txt; } protected set { _txt = value; } }

		private static void PickupConstantSymbols<T>(ref HashSet<ZSymbol> symbols, bool lookupText = false)
			where T : ZSymbol
		{
			Type type = typeof(T);

			FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

			Type codeType = type.GetNestedType("Code");

			// Pickup constant symbols
			foreach (FieldInfo symbolField in fields.Where(f => typeof(ZSymbol).IsAssignableFrom(f.FieldType)))
			{

				FieldInfo symbolCodeField = codeType.GetField(symbolField.Name);
				if (symbolCodeField != null)
				{
					var symbolNumber = (int)symbolCodeField.GetValue(null);

					var symbol = Activator.CreateInstance(typeof(T), new object[] {
						symbolNumber,
						symbolCodeField.Name,
						lookupText ? Marshal.PtrToStringAnsi(zmq.strerror(symbolNumber)) : string.Empty
					});
					symbolField.SetValue(null, symbol);
					symbols.Add((ZSymbol)symbol);
				}
			}
		}

		public static readonly ZSymbol None = default(ZSymbol);

		static ZSymbol()
		{
			// System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(zmq).TypeHandle);

			var symbols = new HashSet<ZSymbol>();

			PickupConstantSymbols<ZError>(ref symbols, true);

			_allSymbols = symbols.ToArray();
		}

		public bool IsEmpty()
		{
			return this == default(ZSymbol);
		}

		static readonly ZSymbol[] _allSymbols;

		public static IEnumerable<ZSymbol> Find(string symbol)
		{
			return _allSymbols
				.Where(s => s.Name == null ? false : (s.Name == symbol));
		}

		public static IEnumerable<ZSymbol> Find(string ns, int num)
		{
			return _allSymbols
				.Where(s => s.Name == null ? false : (s.Name.StartsWith(ns) && s.Number == num));
		}

		public override bool Equals(object obj)
		{
			return ZSymbol.Equals(this, obj);
		}

		public static new bool Equals(object a, object b)
		{
			if (object.ReferenceEquals(a, b))
			{
				return true;
			}

			var symbolA = a as ZSymbol;
			if (symbolA == null)
			{
				return false;
			}

			var symbolB = b as ZSymbol;
			if (symbolB == null)
			{
				return false;
			}

			return symbolA.GetHashCode() == symbolB.GetHashCode();
		}

		public override int GetHashCode()
		{
			int hash = Number.GetHashCode();

			if (Name != null)
				hash ^= Name.GetHashCode();

			return hash;
		}

		public override string ToString()
		{
			return (Name == null ? string.Empty : Name) + "(" + Number + "): " + Text;
		}

		public static implicit operator int(ZSymbol errnum)
		{
			return errnum == null ? -1 : errnum.Number;
		}

	}
}