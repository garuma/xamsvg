using System;
using Android.Graphics;

namespace XamSvg.Internals.Utils
{
	public class SVGLineParser
	{
		public static void parse(SVGProperties pSVGProperties, Canvas pCanvas, SVGPaint pSVGPaint) {
			float x1 = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_X1, 0f);
			float x2 = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_X2, 0f);
			float y1 = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_Y1, 0f);
			float y2 = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_Y2, 0f);
			if (pSVGPaint.setStroke(pSVGProperties)) {
				pSVGPaint.ensureComputedBoundsInclude(x1, y1);
				pSVGPaint.ensureComputedBoundsInclude(x2, y2);
				pCanvas.DrawLine(x1, y1, x2, y2, pSVGPaint.getPaint());
			}
		}
	}
}
