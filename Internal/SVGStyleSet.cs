using System;
using System.Collections.Generic;

namespace XamSvg.Internals
{
	public class SVGStyleSet
	{
		Dictionary<string, string> mStyleMap = new Dictionary<string, string> ();

		public SVGStyleSet(string pString)
		{
			foreach (var pair in pString.Split (';')) {
				var kvp = pair.Split (':');
				if (kvp.Length == 2)
					mStyleMap[kvp[0]] = kvp[1];
			}
		}

		public string GetStyle (string pStyleName)
		{
			string style;
			return mStyleMap.TryGetValue (pStyleName, out style) ? style : null;
		}
	}
}
