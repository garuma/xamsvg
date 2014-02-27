using System;
using Android.Graphics;
using System.Globalization;

using Attributes = System.Collections.Generic.Dictionary<string, string>;

namespace XamSvg.Internals.Utils
{
	using Utils;

	public class SVGParserUtils
	{
		public static float? extractFloatAttribute(String pString)
		{
			if (pString == null)
				return null;
			float value;
			if (pString.EndsWith(SVGConstants.UNIT_PX))
				pString = pString.Substring (0, pString.Length - SVGConstants.UNIT_PX.Length);
			return float.TryParse (pString, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : (float?)null;
		}

		public static String extractIDFromURLProperty(String pProperty)
		{
			var start = "url(#".Length;
			return pProperty.Substring (start, pProperty.Length - start - 1);
		}

		public static Color? extractColorFromRGBProperty(String pProperty) {
			var svgNumberParserIntegerResult = pProperty.Substring("rgb(".Length, pProperty.IndexOf(')')).ParseInts ();
			if(svgNumberParserIntegerResult.Length == 3) {
				return Color.Argb(0, svgNumberParserIntegerResult[0], svgNumberParserIntegerResult[1], svgNumberParserIntegerResult[2]);
			} else {
				return (Color?)null;
			}
		}

		public static Color extraColorIntegerProperty(String pProperty) {
			return new Color (Convert.ToInt32 (pProperty, 16));
		}

		public static Color? extractColorFromHexProperty(String pProperty) {
			String hexColorString = pProperty.Substring(1).Trim();
			if(hexColorString.Length == 3) {
				int parsedInt = Convert.ToInt32(hexColorString, 16);
				int red = (parsedInt & ColorUtils.COLOR_MASK_12BIT_RGB_R) >> 8;
				int green = (parsedInt & ColorUtils.COLOR_MASK_12BIT_RGB_G) >> 4;
				int blue = (parsedInt & ColorUtils.COLOR_MASK_12BIT_RGB_B) >> 0;
				/* Generate color, duplicating the bits, so that i.e.: #F46 gets #FFAA66. */
				return Color.Argb(0xff, (red << 4) | red, (green << 4) | green, (blue << 4) | blue);
			} else if(hexColorString.Length == 6) {
				return new Color ((0xff << 24) | Convert.ToInt32(hexColorString, 16));
			} else {
				return (Color?)null;
			}
		}

		public static String parseHref(Attributes pAttributes) {
			String href = pAttributes.GetStringAttribute(SVGConstants.ATTRIBUTE_HREF);
			if(href != null) {
				if(href.StartsWith("#")) {
					href = href.Substring(1);
				}
			}
			return href;
		}
	}
}
