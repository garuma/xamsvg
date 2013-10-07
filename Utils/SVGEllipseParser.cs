using System;
using Android.Graphics;

namespace XamSvg.Internals.Utils
{
	public class SVGEllipseParser
	{
		public static void parse(SVGProperties pSVGProperties, Canvas pCanvas, SVGPaint pSVGPaint, RectF pRect) {
			float? centerX = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_CENTER_X);
			float? centerY = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_CENTER_Y);
			float? radiusX = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_RADIUS_X);
			float? radiusY = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_RADIUS_Y);
			if (centerX != null && centerY != null && radiusX != null && radiusY != null) {
				pRect.Set(centerX.Value - radiusX.Value,
				          centerY.Value - radiusY.Value,
				          centerX.Value + radiusX.Value,
				          centerY.Value + radiusY.Value);

				bool fill = pSVGPaint.setFill(pSVGProperties);
				if (fill) {
					pCanvas.DrawOval(pRect, pSVGPaint.getPaint());
				}

				bool stroke = pSVGPaint.setStroke(pSVGProperties);
				if (stroke) {
					pCanvas.DrawOval(pRect, pSVGPaint.getPaint());
				}

				if(fill || stroke) {
					pSVGPaint.ensureComputedBoundsInclude(centerX.Value - radiusX.Value, centerY.Value - radiusY.Value);
					pSVGPaint.ensureComputedBoundsInclude(centerX.Value + radiusX.Value, centerY.Value + radiusY.Value);
				}
			}
		}

	}
}
