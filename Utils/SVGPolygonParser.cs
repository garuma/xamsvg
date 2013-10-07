using System;
using Android.Graphics;
using Android.Util;

namespace XamSvg.Internals.Utils
{
	public class SVGPolygonParser
	{
		public static void parse(SVGProperties pSVGProperties, Canvas pCanvas, SVGPaint pSVGPaint) {
			var points = pSVGProperties.GetStringAttribute(SVGConstants.ATTRIBUTE_POINTS).ParseFloats ();
			if (points != null) {
				if (points.Length >= 2) {
					Path path = SVGPolylineParser.parse(points);
					path.Close();

					bool fill = pSVGPaint.setFill(pSVGProperties);
					if (fill) {
						pCanvas.DrawPath(path, pSVGPaint.getPaint());
					}

					bool stroke = pSVGPaint.setStroke(pSVGProperties);
					if (stroke) {
						pCanvas.DrawPath(path, pSVGPaint.getPaint());
					}

					if(fill || stroke) {
						pSVGPaint.ensureComputedBoundsInclude(path);
					}
				}
			}
		}

	}
}
