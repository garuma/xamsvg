using System;
using Android.Graphics;

namespace XamSvg.Internals.Utils
{
	public class ColorUtils
	{
		public const uint COLOR_MASK_32BIT_ARGB_ALPHA = 0xFF000000;
		public const int COLOR_MASK_32BIT_ARGB_RGB = 0xFFFFFF;
		public const int COLOR_MASK_32BIT_ARGB_R = 0xFF0000;
		public const int COLOR_MASK_32BIT_ARGB_G = 0x00FF00;
		public const int COLOR_MASK_32BIT_ARGB_B = 0x0000FF;

		public const int COLOR_MASK_12BIT_RGB_R = 0xF00;
		public const int COLOR_MASK_12BIT_RGB_G = 0x0F0;
		public const int COLOR_MASK_12BIT_RGB_B = 0x00F;

		//public static final Pattern RGB_PATTERN = Pattern.compile("rgb\\((.*[\\d]+),.*([\\d]+),.*([\\d]+).*\\)");

		public static Color? GetColorByName(string colorName)
		{
			try {
				return Color.ParseColor (colorName);
			} catch {
				return null;
			}
		}
	}
}
