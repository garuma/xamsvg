using System;

namespace XamSvg.Internals
{
	public class SVGParseException : Exception
	{
		public SVGParseException (string msg) : base (msg)
		{

		}

		public SVGParseException (string msg, Exception inner) : base (msg, inner)
		{

		}

		public SVGParseException (Exception inner) : base ("SVGParseException", inner)
		{

		}
	}
}
