using System;
using System.IO;

using Android.Graphics;
using Android.Graphics.Drawables;

namespace XamSvg
{
	using Internals;

	public static class SvgFactory
	{
		public static Bitmap GetBitmap (Android.Content.Res.Resources resources,
		                                int resID,
		                                int width, int height,
		                                ISvgColorMapper colorMapper = null)
		{
			var svg = SVGParser.ParseSVGFromResource (resources, resID, colorMapper);
			return MakeBitmapFromSvg (svg, width, height);
		}

		public static Bitmap GetBitmap (string svgString, int width, int height,
		                                ISvgColorMapper colorMapper = null)
		{
			var svg = SVGParser.ParseSVGFromString (svgString, colorMapper);
			return MakeBitmapFromSvg (svg, width, height);
		}

		public static Bitmap GetBitmap (TextReader reader, int width, int height,
		                                ISvgColorMapper colorMapper = null)
		{
			var svg = SVGParser.ParseSvgFromReader (reader, colorMapper);
			return MakeBitmapFromSvg (svg, width, height);
		}

		public static Bitmap GetBitmap (Stream stream, int width, int height,
		                                ISvgColorMapper colorMapper = null)
		{
			var svg = SVGParser.ParseSvgFromStream (stream, colorMapper);
			return MakeBitmapFromSvg (svg, width, height);
		}

		internal static Bitmap MakeBitmapFromSvg (Svg svg, int width, int height)
		{
			return MakeBitmapFromPicture (svg.Picture, width, height);
		}

		internal static Bitmap MakeBitmapFromPicture (Picture pic, int width, int height)
		{
			using (var bmp = Bitmap.CreateBitmap (width, height, Bitmap.Config.Argb8888)) {
				using (var c = new Canvas (bmp)) {
					var dst = new RectF (0, 0, width, height); 
					c.DrawPicture (pic, dst);
				}
				// Returns an immutable copy
				return Bitmap.CreateBitmap (bmp);
			}
		}

		public static PictureBitmapDrawable GetDrawable (Android.Content.Res.Resources resources,
		                                                 int resID,
		                                                 ISvgColorMapper colorMapper = null)
		{
			var svg = SVGParser.ParseSVGFromResource (resources, resID, colorMapper);
			return new PictureBitmapDrawable (svg.Picture);
		}

		public static PictureBitmapDrawable GetDrawable (string svgString, ISvgColorMapper colorMapper = null)
		{
			var svg = SVGParser.ParseSVGFromString (svgString, colorMapper);
			return new PictureBitmapDrawable (svg.Picture);
		}

		public static PictureBitmapDrawable GetDrawable (TextReader reader, ISvgColorMapper colorMapper = null)
		{
			var svg = SVGParser.ParseSvgFromReader (reader, colorMapper);
			return new PictureBitmapDrawable (svg.Picture);
		}

		public static PictureBitmapDrawable GetDrawable (Stream stream, ISvgColorMapper colorMapper = null)
		{
			var svg = SVGParser.ParseSvgFromStream (stream, colorMapper);
			return new PictureBitmapDrawable (svg.Picture);
		}
	}
}

