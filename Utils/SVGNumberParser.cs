using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace XamSvg.Internals.Utils
{
	public static class SVGNumberParser {

		public static float[] ParseFloats (this string str)
		{
			if (str == null)
				return null;
			return Regex.Split (str, "[\\s,]+").Select (float.Parse).ToArray ();
		}

		public static int[] ParseInts (this string str)
		{
			if (str == null)
				return null;
			return Regex.Split (str, "[\\s,]+").Select (int.Parse).ToArray ();
		}
	}
}
