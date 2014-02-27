using System;
using System.Globalization;

namespace XamSvg
{
	public static class FloatUtils
	{
		static readonly System.Globalization.CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;

		public static float ToSafeFloat (this string str)
		{
			if (string.IsNullOrEmpty (str))
				return 0;
			return float.Parse (str, invariantCulture.NumberFormat);
		}

		public static double ToSafeDouble (this string str)
		{
			if (string.IsNullOrEmpty (str))
				return 0;
			return double.Parse (str, invariantCulture.NumberFormat);
		}
	}
}

