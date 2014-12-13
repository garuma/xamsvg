using System;
using System.IO;
using System.Xml;

using Android.Content.Res;
using Android.Graphics;

namespace XamSvg
{
	using Internals;

	public class SVGParser
	{
		public static Svg ParseSVGFromString (string pString)
		{
			return ParseSVGFromString(pString, null);
		}

		public static Svg ParseSVGFromString (string pString, ISvgColorMapper pSVGColorMapper)
		{
			return ParseSvgFromReader(new StringReader(pString), pSVGColorMapper);
		}

		public static Svg ParseSVGFromResource (Resources pResources, int pRawResourceID)
		{
			return ParseSVGFromResource(pResources, pRawResourceID, null);
		}

		public static Svg ParseSVGFromResource (Resources pResources, int pRawResourceID, ISvgColorMapper pSVGColorMapper)
		{
			return ParseSvgFromStream(pResources.OpenRawResource (pRawResourceID), pSVGColorMapper);
		}

		public static Svg ParseSvgFromAsset (AssetManager pAssetManager, string pAssetPath)
		{
			return ParseSvgFromAsset(pAssetManager, pAssetPath, null);
		}

		public static Svg ParseSvgFromAsset (AssetManager pAssetManager, string pAssetPath, ISvgColorMapper pSVGColorMapper)
		{
			using (var stream = pAssetManager.Open (pAssetPath))
				return ParseSvgFromStream (stream, pSVGColorMapper);
		}

		public static Svg ParseSvgFromStream(Stream pInputStream, ISvgColorMapper pSVGColorMapper)
		{
			return ParseSvgFromReader (new StreamReader (pInputStream), pSVGColorMapper);
		}

		public static Svg ParseSvgFromReader (TextReader reader, ISvgColorMapper pSVGColorMapper)
		{
			try {
				var readerSettings = new XmlReaderSettings();
				readerSettings.XmlResolver = null;
				readerSettings.DtdProcessing = DtdProcessing.Ignore;
				var xmlReader = XmlReader.Create (reader, readerSettings);
				Picture picture = new Picture();
				var svgHandler = new SVGHandler (picture, pSVGColorMapper);
				svgHandler.Parse (xmlReader);
				Svg svg = new Svg(picture, svgHandler.getBounds(), svgHandler.getComputedBounds());
				return svg;
			} catch (Exception e) {
				throw new SVGParseException(e);
			}
		}
	}
}
