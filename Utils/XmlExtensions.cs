using System;
using System.Xml;

using Attributes = System.Collections.Generic.Dictionary<string, string>;

namespace XamSvg.Internals.Utils
{
	public static class XmlExtensions
	{
		public static string GetStringAttribute (this Attributes attributes, string name, string defaultValue = null)
		{
			string attr;
			if (!attributes.TryGetValue (name, out attr))
				return defaultValue;
			return attr;
		}

		public static float? GetFloatAttribute (this Attributes attributes, string name, float? defaultValue = null)
		{
			return SVGParserUtils.extractFloatAttribute (attributes.GetStringAttribute (name)) ?? defaultValue;
		}
	}
}

