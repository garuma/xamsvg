using System;
using Android.Graphics;

namespace XamSvg.Internals.Utils
{
	public class SVGCircleParser
	{
		public static void Parse(SVGProperties pSVGProperties, Canvas pCanvas, SVGPaint pSVGPaint)
		{
			float? centerX = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_CENTER_X);
			float? centerY = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_CENTER_Y);
			float? radius = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_RADIUS);
			if (centerX != null && centerY != null && radius != null) {
				bool fill = pSVGPaint.setFill(pSVGProperties);
				if (fill) {
					pCanvas.DrawCircle(centerX.Value, centerY.Value, radius.Value, pSVGPaint.getPaint());
				}

				bool stroke = pSVGPaint.setStroke(pSVGProperties);
				if (stroke) {
					pCanvas.DrawCircle(centerX.Value, centerY.Value, radius.Value, pSVGPaint.getPaint());
				}

				if(fill || stroke) {
					pSVGPaint.ensureComputedBoundsInclude(centerX.Value - radius.Value, centerY.Value - radius.Value);
					pSVGPaint.ensureComputedBoundsInclude(centerX.Value + radius.Value, centerY.Value + radius.Value);
				}
			}
		}
	}
}
