using System;
using Android.Graphics;
using Android.Util;

namespace XamSvg.Internals.Utils
{
	public class SVGPolylineParser {

		public static void parse(SVGProperties pSVGProperties, Canvas pCanvas, SVGPaint pSVGPaint) {
			var points = pSVGProperties.GetStringAttribute(SVGConstants.ATTRIBUTE_POINTS).ParseFloats ();
			if (points != null) {
				if (points.Length >= 2) {
					Path path = SVGPolylineParser.parse(points);

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

		internal static Path parse(float[] pPoints) {
			Path path = new Path();
			path.MoveTo(pPoints[0], pPoints[1]);
			for (int i = 2; i < pPoints.Length; i += 2) {
				float x = pPoints[i];
				float y = pPoints[i + 1];
				path.LineTo(x, y);
			}
			return path;
		}
	}
}
